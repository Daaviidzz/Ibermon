using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPartidas : MonoBehaviour
{
    [Header("Paneles (arrastra desde el Inspector)")]
    public GameObject panelPartidas;            // El panel con la lista de partidas
    public GameObject panelPartida;             // El panel con el InputField del nombre

    [Header("Texto de carga")]
    public TextMeshProUGUI textoCarga;          // El texto "Cargando..." del PanelCarga
    public GameObject panelEleccionDePersonaje; // El panel con los dos botones de personaje

    // Nombre que el jugador escribe en PanelPartida.
    // Lo guarda el InputField llamando a GuardarNombre().
    private string _nombrePartida = "Mi Partida";
    public GameObject panelCarga;               // Pantalla negra con "Cargando..."

    [Header("Personaje")]
    public CrearYPosicionarPlayer creadorPersonaje;

    [Header("Lista de partidas")]
    public Transform contenedorPartidas;        // El Content del ScrollView

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
    public GameObject prefabPartida;            // El prefab de cada fila de partida
}
// Controla la escena "Partidas": lista las partidas del usuario, deja crear una nueva
// o cargar una existente.
// Ver MANUAL_ESCENAS_UNITY.md → Sección "ESCENA 3 — Partidas" para la estructura de UI.
public class MenuPartida : MonoBehaviour
{
    public Button        botonNueva;
    public Button        botonVolver;
    public TMP_Dropdown  dropdownPersonaje;
    public TMP_Dropdown  dropdownStarter;
    public Button        botonConfirmarNueva;
    public Button        botonCancelarNueva;

    [Header("Escenas")]
    public string escenaJuego = "PuebloFuenlabrada";

    [Header("Referencia Player")]
    [Tooltip("El SpawnHelper de la escena — solo se usa para partidas nuevas")]
    public CrearYPosicionarPlayer posicionarPlayer;

    // IDs del catálogo de starters — ajustar si el seed de la API usa otros números
    // 0 → Ignifor (1), 1 → Aquillo (4), 2 → Verdino (7)
    private readonly int[] _starterCatalogoIds = { 1, 4, 7 };

    private void CargarListaPartidas()
    {
        ApiSetup.Partida.ListarPartidas(
            lista =>
            {
                LimpiarContenedor();

                // Si no tiene partidas ir directo a crear una
                if (lista.Count == 0)
                {
                    MostrarPanelNueva();
                    return;
                }

                MostrarPanelLista();
                foreach (var resumen in lista)
                    CrearEntradaPartida(resumen);

                EventSystem.current?.SetSelectedGameObject(
                    contenedorPartidas.childCount > 0
                        ? contenedorPartidas.GetChild(0).gameObject
                        : null);
            },
            err =>
            {
                Debug.LogError($"[MenuPartida] Error listando partidas: {err}");

                // Si el token expiró mandar al login
                if (err.Contains("[401]"))
                {
                    ApiSetup.Auth.Logout();
                    SceneManager.LoadScene("Login");
                    return;
                }

                MostrarPanelLista();
            });
    }

    private void CrearEntradaPartida(PartidaResumen resumen)
    {
        var go    = Instantiate(prefabEntradaPartida, contenedorPartidas);
        var entry = go.GetComponent<PartidaEntryUI>();
        if (entry == null)
        {
            Debug.LogError("[MenuPartida] El prefab 'prefabEntradaPartida' no tiene el componente PartidaEntryUI. " +
                           "Revisa que el prefab correcto está asignado en el Inspector.");
            Destroy(go);
            return;
        }
        entry.Inicializar(
            resumen,
            onSeleccionar: r => CargarPartida(r.id),
            onEliminar:    r => EliminarPartida(r.id, go)
        );
    }

    private void CargarPartida(string partidaId)
    {
        MostrarCargando("Cargando partida...");

        ApiSetup.Partida.ObtenerPartida(partidaId,
            partida =>
            {
                MostrarCargando("Cargando equipo...");
                ApiSetup.IbermonJugador.ObtenerEquipo(partidaId,
                    equipo =>
                    {
                        SessionManager.Instance.IniciarConPartida(partida, equipo);

                        // Guardar la posición exacta de la API y activar el flag para que
                        // JugadorSpawn la use aunque sea (0,0)
                        JugadorSpawn.posicion            = new Vector2(partida.posicion.x, partida.posicion.y);
                        JugadorSpawn.usarPosicionGuardada = true;

                        // Ojo: no usar posicionarPlayer aquí o sobreescribiría la posición guardada
                        SpawnPlayerSiNoExiste();
                        SceneManager.LoadScene(string.IsNullOrEmpty(partida.mapa_actual) ? escenaJuego : partida.mapa_actual);
                    },
                    err => ManejarErrorCarga(err));
            },
            err => ManejarErrorCarga(err));
    }

    // Conectado al botón "Confirmar" del panel nueva partida
    public void OnClickConfirmarNueva()
    {
        string personaje  = dropdownPersonaje.value == 0 ? "chico" : "chica";
        int    starterIdx = Mathf.Clamp(dropdownStarter.value, 0, _starterCatalogoIds.Length - 1);
        int    starterId  = _starterCatalogoIds[starterIdx];

        MostrarCargando("Creando partida...");

        ApiSetup.Partida.CrearPartida(personaje, starterId,
            partida =>
            {
                MostrarCargando("Cargando equipo inicial...");
                ApiSetup.IbermonJugador.ObtenerEquipo(partida.id,
                    equipo =>
                    {
                        SessionManager.Instance.IniciarConPartida(partida, equipo);
                        JugadorSpawn.posicion = Vector2.zero;
                        IniciarJuego(partida.mapa_actual ?? escenaJuego);
                    },
                    err =>
                    {
                        // A veces el starter tarda un poco en registrarse, arrancamos igual
                        SessionManager.Instance.IniciarConPartida(partida, new List<IbermonJugador>());
                        IniciarJuego(partida.mapa_actual ?? escenaJuego);
                    });
            },
            err =>
            {
                MostrarPanelNueva();
                Debug.LogError($"[MenuPartida] Error creando partida: {err}");
            });
    }

    private void EliminarPartida(string partidaId, GameObject entrada)
    {
        // TODO: añadir un popup de confirmación antes de borrar
        MostrarCargando("Eliminando partida...");
        ApiSetup.Partida.EliminarPartida(partidaId,
            () =>
            {
                Destroy(entrada);
                MostrarCargando("Cargando partidas...");
                CargarListaPartidas();
            },
            err =>
            {
                Debug.LogError($"[MenuPartida] Error eliminando: {err}");
                MostrarPanelLista();
            });
    }

    // Solo para partidas nuevas — usa posicionarPlayer para la posición inicial
    // Para partidas cargadas se usa CargarPartida() directamente
    private void IniciarJuego(string escena)
    {
        string nombreEscena = string.IsNullOrEmpty(escena) ? escenaJuego : escena;

        if (posicionarPlayer != null)
        {
            posicionarPlayer.escenaDestino = nombreEscena;
            posicionarPlayer.crearEInstanciarPersonaje();
        }
        else
        {
            SceneManager.LoadScene(nombreEscena);
        }
    }

    // Instancia el prefab del jugador solo si todavía no existe
    private void SpawnPlayerSiNoExiste()
    {
        if (GameObject.FindWithTag("Player") == null && posicionarPlayer != null)
            Instantiate(posicionarPlayer.personaje);
    }

    private void ManejarErrorCarga(string err)
    {
        Debug.LogError($"[MenuPartida] Error: {err}");
        if (err.Contains("[401]"))
        {
            ApiSetup.Auth.Logout();
            SceneManager.LoadScene("Login");
            return;
        }
        MostrarPanelLista();
    }

    private void MostrarPanelLista()
    {
        panelLista.SetActive(true);
        panelNuevaPartida.SetActive(false);
        panelCargando.SetActive(false);
    }

    private void InicializarDropdowns()
    {
        if (dropdownPersonaje != null)
        {
            dropdownPersonaje.ClearOptions();
            dropdownPersonaje.AddOptions(new List<string> { "Chico", "Chica" });
        }

        if (dropdownStarter != null)
        {
            dropdownStarter.ClearOptions();
            dropdownStarter.AddOptions(new List<string>
            {
                "Ignifor (Fuego)",
                "Aquillo (Agua)",
                "Verdino (Planta)"
            });
        }
    }
}
