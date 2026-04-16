using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Controla toda la escena de Partidas.
// Flujo: carga partidas → si tiene, muestra lista → si no, va directo a crear nueva.
// Para crear nueva: PanelPartida (nombre) → PanelEleccionDePersonaje → juego.
public class MenuPartidas : MonoBehaviour
{
    [Header("Paneles (arrastra desde el Inspector)")]
    public GameObject panelPartidas;            // El panel con la lista de partidas
    public GameObject panelPartida;             // El panel con el InputField del nombre
    public GameObject panelEleccionDePersonaje; // El panel con los dos botones de personaje
    public GameObject panelCarga;               // Pantalla negra con "Cargando..."

    [Header("Lista de partidas")]
    public Transform contenedorPartidas;        // El Content del ScrollView
    public GameObject prefabPartida;            // El prefab de cada fila de partida

    [Header("Texto de carga")]
    public TextMeshProUGUI textoCarga;          // El texto "Cargando..." del PanelCarga

    // Nombre que el jugador escribe en PanelPartida.
    // Lo guarda el InputField llamando a GuardarNombre().
    private string _nombrePartida = "Mi Partida";

    [Header("Personaje")]
    public CrearYPosicionarPlayer creadorPersonaje;

    // ─────────────────────────────────────────────────────────
    //  INICIO
    // ─────────────────────────────────────────────────────────

    private void Start()
    {
        // Liberar el cursor por si venía del juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MostrarCarga("Cargando partidas...");
        CargarPartidas();
    }

    // ─────────────────────────────────────────────────────────
    //  CARGAR LISTA DE PARTIDAS
    // ─────────────────────────────────────────────────────────

    private void CargarPartidas()
    {
        ApiSetup.Partida.ListarPartidas(
            onSuccess: lista =>
            {
                // Vaciar el contenedor antes de rellenar
                LimpiarLista();

                if (lista.Count == 0)
                {
                    // Sin partidas → ir directo a crear una nueva
                    MostrarPanel(panelPartida);
                    return;
                }

                // Hay partidas → mostrar la lista
                foreach (var partida in lista)
                    CrearEntradaEnLista(partida);

                MostrarPanel(panelPartidas);
            },
            onError: error =>
            {
                Debug.LogError($"Error al cargar partidas: {error}");

                // Si el token expiró, volver al login
                if (error.Contains("401"))
                {
                    SceneManager.LoadScene("Login");
                    return;
                }

                // Cualquier otro error: mostrar la lista vacía
                MostrarPanel(panelPartidas);
            });
    }

    // Instancia un prefab por cada partida y lo configura
    private void CrearEntradaEnLista(PartidaResumen partida)
    {
        var entrada = Instantiate(prefabPartida, contenedorPartidas);
        var script = entrada.GetComponent<PartidaEntryUI>();

        script.Inicializar(
            partida,
            onSeleccionar: CargarPartida,   // Al pulsar la entrada → cargar esa partida
            onEliminar: EliminarPartida  // Al pulsar la X → borrarla
        );
    }

    private void LimpiarLista()
    {
        foreach (Transform hijo in contenedorPartidas)
            Destroy(hijo.gameObject);
    }

    // ─────────────────────────────────────────────────────────
    //  CARGAR UNA PARTIDA EXISTENTE
    // ─────────────────────────────────────────────────────────

    private void CargarPartida(PartidaResumen resumen)
    {
        MostrarCarga("Cargando partida...");

        // Primero obtenemos los datos completos de la partida
        ApiSetup.Partida.ObtenerPartida(resumen.id,
            onSuccess: partida =>
            {
                MostrarCarga("Guardando progreso...");

                // Construimos el body del PUT con todos los campos que pide la API,
                // solo incrementamos tiempo_jugado en 1 (lo usamos como contador de entradas)
                var datosGuardar = new GuardarPartidaRequest
                {
                    mapa_actual = partida.mapa_actual,
                    posicion = partida.posicion,
                    dinero = partida.dinero,
                    tiempo_jugado = partida.tiempo_jugado + 1, // +1 cada vez que se entra
                    pokedex_visto = partida.pokedex_visto,
                    pokedex_capturado = partida.pokedex_capturado,
                    medallas = partida.medallas,
                    logros = partida.logros,
                    combates_ganados = partida.combates_ganados,
                    combates_perdidos = partida.combates_perdidos,
                    flags = partida.flags,
                };

                ApiSetup.Partida.GuardarPartida(partida.id, datosGuardar,
onSuccess: partidaActualizada =>
{
    SessionManager.Instance.IniciarConPartida(partidaActualizada, new List<IbermonJugador>());
    string escena = string.IsNullOrEmpty(partidaActualizada.mapa_actual) ? "CasaPersonaje" : partidaActualizada.mapa_actual;
    creadorPersonaje.escenaDestino = escena; // por si la escena varía según la partida
    creadorPersonaje.crearEInstanciarPersonaje();
},
onError: error =>
{
    Debug.LogWarning($"No se pudo incrementar el contador: {error}");
    SessionManager.Instance.IniciarConPartida(partida, new List<IbermonJugador>());
    string escena = string.IsNullOrEmpty(partida.mapa_actual) ? "CasaPersonaje" : partida.mapa_actual;
    creadorPersonaje.escenaDestino = escena;
    creadorPersonaje.crearEInstanciarPersonaje();
});
            },
            onError: error =>
            {
                Debug.LogError($"Error al cargar partida: {error}");
                MostrarPanel(panelPartidas);
            });
    }

    // ─────────────────────────────────────────────────────────
    //  ELIMINAR UNA PARTIDA
    // ─────────────────────────────────────────────────────────

    private void EliminarPartida(PartidaResumen resumen)
    {
        MostrarCarga("Eliminando partida...");

        ApiSetup.Partida.EliminarPartida(resumen.id,
            onSuccess: () =>
            {
                // Recargar la lista tras borrar
                CargarPartidas();
            },
            onError: error =>
            {
                Debug.LogError($"Error al eliminar partida: {error}");
                MostrarPanel(panelPartidas);
            });
    }

    // ─────────────────────────────────────────────────────────
    //  CREAR NUEVA PARTIDA (flujo de 2 pasos)
    // ─────────────────────────────────────────────────────────

    // Llamado por el InputField del PanelPartida con OnValueChanged o por el botón Continuar.
    // Conecta el InputField al Inspector o llama este método desde un botón con SendMessage.
    public void GuardarNombre(string nombre)
    {
        // Guardamos el nombre aunque de momento no lo enviemos a la API
        _nombrePartida = string.IsNullOrWhiteSpace(nombre) ? "Mi Partida" : nombre;
    }

    // Botón "Continuar" del PanelPartida → ir a elegir personaje
    public void OnContinuar()
    {
        MostrarPanel(panelEleccionDePersonaje);
    }

    // Botón "Torrente" del PanelEleccionDePersonaje
    public void OnElegirTorrente()
    {
        CrearPartidaConPersonaje("torrente");
    }

    // Botón "Personaje1" del PanelEleccionDePersonaje
    public void OnElegirPersonaje1()
    {
        CrearPartidaConPersonaje("personaje1");
    }

    // Llama a la API para crear la partida y arranca el juego
    private void CrearPartidaConPersonaje(string personaje)
    {
        MostrarCarga("Creando partida...");

        // Starter hardcodeado a 1 de momento (luego lo haremos bien)
        const int starterHardcodeado = 1;

        ApiSetup.Partida.CrearPartida(personaje, starterHardcodeado,
onSuccess: partida =>
{
    SessionManager.Instance.IniciarConPartida(partida, new List<IbermonJugador>());
    creadorPersonaje.escenaDestino = "CasaPersonaje";
    creadorPersonaje.crearEInstanciarPersonaje();
},
            onError: error =>
            {
                Debug.LogError($"Error al crear partida: {error}");
                MostrarPanel(panelEleccionDePersonaje);
            });
    }

    // ─────────────────────────────────────────────────────────
    //  BOTONES GENERALES
    // ─────────────────────────────────────────────────────────

    // Botón "Nueva" del PanelPartidas → ir a escribir el nombre
    public void OnNuevaPartida()
    {
        MostrarPanel(panelPartida);
    }

    // Botón "Volver" de cualquier panel → volver al menú principal
    public void OnVolver()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    // ─────────────────────────────────────────────────────────
    //  HELPERS DE PANELES
    // ─────────────────────────────────────────────────────────

    // Muestra solo el panel indicado y oculta el resto
    private void MostrarPanel(GameObject panel)
    {
        panelPartidas.SetActive(panel == panelPartidas);
        panelPartida.SetActive(panel == panelPartida);
        panelEleccionDePersonaje.SetActive(panel == panelEleccionDePersonaje);
        panelCarga.SetActive(false);
    }

    // Muestra la pantalla de carga con el mensaje indicado
    private void MostrarCarga(string mensaje)
    {
        panelPartidas.SetActive(false);
        panelPartida.SetActive(false);
        panelEleccionDePersonaje.SetActive(false);
        panelCarga.SetActive(true);

        if (textoCarga != null)
            textoCarga.text = mensaje;
    }
}