using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Este script controla la escena donde el jugador elige que partida cargar
// o crea una nueva eligiendo nombre y personaje
// Flujo general:
//   1. Al entrar pide al servidor la lista de partidas
//   2. Si hay partidas muestra la lista, si no va directo a crear una nueva
//   3. Para crear una nueva el jugador escribe el nombre y elige personaje
//   4. Al pulsar una partida de la lista se abre el panel de detalle
//   5. Desde el detalle se puede jugar o borrar la partida
public class MenuPartidas : MonoBehaviour
{
    // Paneles que se activan o desactivan segun lo que vaya haciendo el jugador
    [Header("Paneles")]
    public GameObject panelPartidas;            // Lista de partidas guardadas
    public GameObject panelNuevaPartida;        // Formulario de nombre
    public GameObject panelEleccionDePersonaje; // Eleccion de personaje
    public GameObject panelDetallePartida;      // Detalle de una partida concreta
    public GameObject panelCarga;

    // Componentes necesarios para mostrar la lista de partidas del jugador
    [Header("Lista de partidas")]
    public Transform contenedorPartidas;
    public GameObject prefabPartida;

    // Texto que aparece en el panel de carga con el mensaje actual
    [Header("Texto de carga")]
    public TextMeshProUGUI textoCarga;

    // Textos del panel de detalle de partida
    [Header("Panel detalle")]
    public TextMeshProUGUI detalleNombre;         // Nombre de la partida
    public TextMeshProUGUI detallePersonaje;      // Personaje elegido
    public TextMeshProUGUI detalleTiempo;         // Tiempo jugado formateado
    public TextMeshProUGUI detalleFechaCreacion;  // Fecha en que se creo la partida
    public TextMeshProUGUI detalleUltimaConexion; // Fecha de la ultima vez que se jugo

    [Header("Nueva partida")]
    public TMP_InputField inputNombrePartida;

    // Componente que se encarga de instanciar al personaje y cambiar de escena
    [Header("Personaje")]
    public CrearYPosicionarPlayer creadorPersonaje;

    // Nombre que el jugador escribe en el panel de nueva partida
    // Lo guarda el InputField llamando a GuardarNombre
    private string _nombrePartidaActual = "Mi Partida";

    // Partida que esta mostrando el panel de detalle en este momento
    private PartidaResumen _partidaEnDetalle;

    // Se ejecuta automaticamente al arrancar la escena
    private void Start()
    {
        // Liberamos el cursor por si venia bloqueado desde el juego
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MostrarCarga("Cargando partidas...");
        CargarPartidas();
    }

    // ── Lista ────────────────────────────────────────────────────────────────

    // Pide al servidor la lista de partidas del usuario y monta la interfaz segun el resultado
    private void CargarPartidas()
    {
        ApiSetup.Partida.ListarPartidas(ManejarListaPartidasRecibida, ManejarErrorListaPartidas);
    }

    // Se ejecuta cuando el servidor devuelve la lista de partidas
    // Si esta vacia pasa directo al panel de creacion de partida
    // Si tiene partidas crea una entrada por cada una y muestra la lista
    private void ManejarListaPartidasRecibida(List<PartidaResumen> listaRecibida)
    {
        LimpiarLista();

        if (listaRecibida.Count == 0)
        {
            MostrarPanel(panelNuevaPartida);
            return;
        }

        foreach (PartidaResumen partida in listaRecibida)
        {
            CrearEntradaEnLista(partida);
        }

        MostrarPanel(panelPartidas);
    }

    // Se ejecuta si hubo un error al pedir la lista al servidor
    // Si el token ha caducado volvemos al login, si no mostramos la lista vacia
    private void ManejarErrorListaPartidas(string mensajeError)
    {
        Debug.LogError($"Error al cargar partidas: {mensajeError}");

        if (mensajeError.Contains("401"))
        {
            SceneManager.LoadScene("Login");
            return;
        }

        MostrarPanel(panelPartidas);
    }

    // Instancia el prefab de una partida en el contenedor y le pasa los callbacks
    private void CrearEntradaEnLista(PartidaResumen partida)
    {
        GameObject entradaInstanciada = Instantiate(prefabPartida, contenedorPartidas);
        PartidaEntryUI controladorEntrada = entradaInstanciada.GetComponent<PartidaEntryUI>();

        // Al pulsar la entrada se abre el panel de detalle
        // Al pulsar el boton de la X se elimina directamente
        controladorEntrada.Inicializar(partida, AbrirDetallePartida, EliminarPartida);
    }

    // Destruye todos los hijos del contenedor de la lista
    // Se usa para vaciar la lista antes de volver a rellenarla
    private void LimpiarLista()
    {
        foreach (Transform hijo in contenedorPartidas)
        {
            Destroy(hijo.gameObject);
        }
    }

    // ── Detalle ──────────────────────────────────────────────────────────────

    // Abre el panel de detalle rellenando todos los datos de la partida seleccionada
    private void AbrirDetallePartida(PartidaResumen resumen)
    {
        _partidaEnDetalle = resumen;

        detalleNombre.text = resumen.nombre;
        detallePersonaje.text = $"Personaje: {resumen.personaje_elegido}";
        detalleTiempo.text = FormatearTiempo(resumen.tiempo_jugado);
        detalleFechaCreacion.text = $"Creada: {FormatearFecha(resumen.fecha_creacion)}";
        detalleUltimaConexion.text = $"Última vez: {FormatearFecha(resumen.ultima_conexion)}";

        MostrarPanel(panelDetallePartida);
    }

    // Boton Jugar del panel de detalle — carga la partida y arranca el juego
    public void OnJugarPartida()
    {
        if (_partidaEnDetalle == null) return;
        CargarPartida(_partidaEnDetalle);
    }

    // Boton Borrar del panel de detalle — elimina la partida y vuelve a la lista
    public void OnBorrarDesdeDetalle()
    {
        if (_partidaEnDetalle == null) return;
        EliminarPartida(_partidaEnDetalle);
    }

    // Boton Volver del panel de detalle — regresa a la lista sin hacer nada
    public void OnVolverDesdeDetalle()
    {
        _partidaEnDetalle = null;
        MostrarPanel(panelPartidas);
    }

    // ── Cargar partida ───────────────────────────────────────────────────────

    // Carga una partida existente a partir de su resumen
    // Pide los datos completos al servidor, inicia la sesion y cambia de escena
    private void CargarPartida(PartidaResumen resumen)
    {
        MostrarCarga("Cargando partida...");
        ApiSetup.Partida.ObtenerPartida(resumen.id, ManejarPartidaCompletaRecibida, ManejarErrorObtenerPartida);
    }

    // Se ejecuta cuando se reciben los datos completos de la partida
    // Pide el equipo a la API antes de iniciar la sesion para que PokemonParty pueda cargarlo
    private void ManejarPartidaCompletaRecibida(PartidaCompleta partidaRecibida)
    {
        ApiSetup.IbermonJugador.ObtenerEquipo(partidaRecibida.id,
            equipoRecibido => IniciarSesionYCargarEscena(partidaRecibida, equipoRecibido),
            mensajeError =>
            {
                // Si falla pedir el equipo seguimos igualmente con lista vacia para no bloquear al jugador
                Debug.LogError($"Error al obtener equipo de la partida: {mensajeError}");
                IniciarSesionYCargarEscena(partidaRecibida, new List<IbermonJugador>());
            });
    }

    // Inicia la sesion con la partida y el equipo recibidos y carga la escena correspondiente
    private void IniciarSesionYCargarEscena(PartidaCompleta partidaRecibida, List<IbermonJugador> equipo)
    {
        SessionManager.Instance.IniciarConPartida(partidaRecibida, equipo);

        creadorPersonaje.personajeElegido = partidaRecibida.personaje_elegido;

        if (string.IsNullOrEmpty(partidaRecibida.mapa_actual))
        {
            creadorPersonaje.escenaDestino = "CasaPersonaje";
        }
        else
        {
            creadorPersonaje.escenaDestino = partidaRecibida.mapa_actual;
        }

        // Cargamos la posicion exacta donde estaba el jugador al guardar
        JugadorSpawn.posicion = new Vector2(partidaRecibida.posicion.x, partidaRecibida.posicion.y);
        // Le decimos a JugadorSpawn que use esa posicion en vez del spawn por defecto
        JugadorSpawn.usarPosicionGuardada = true;

        creadorPersonaje.crearEInstanciarPersonaje();
    }

    // Se ejecuta si no se pudo obtener la partida del servidor
    // Volvemos a mostrar la lista de partidas
    private void ManejarErrorObtenerPartida(string mensajeError)
    {
        Debug.LogError($"Error al cargar partida: {mensajeError}");
        MostrarPanel(panelPartidas);
    }

    // ── Eliminar ─────────────────────────────────────────────────────────────

    // Borra una partida del servidor y recarga la lista
    private void EliminarPartida(PartidaResumen resumen)
    {
        MostrarCarga("Eliminando partida...");
        ApiSetup.Partida.EliminarPartida(resumen.id, ManejarEliminacionExitosa, ManejarEliminacionFallida);
    }

    // Se ejecuta cuando la partida se borra bien
    // Recargamos la lista para que el jugador vea el cambio
    private void ManejarEliminacionExitosa()
    {
        CargarPartidas();
    }

    // Se ejecuta si no se pudo borrar la partida
    private void ManejarEliminacionFallida(string mensajeError)
    {
        Debug.LogError($"Error al eliminar partida: {mensajeError}");
        MostrarPanel(panelPartidas);
    }

    // ── Crear nueva partida ──────────────────────────────────────────────────

    // Metodo que llama el InputField del panel de nueva partida cuando el jugador escribe el nombre
    // Si el jugador no escribe nada dejamos el nombre por defecto
    public void GuardarNombre(string nombreIntroducido)
    {
        if (string.IsNullOrWhiteSpace(nombreIntroducido))
        {
            _nombrePartidaActual = "Mi Partida";
        }
        else
        {
            _nombrePartidaActual = nombreIntroducido;
        }
    }

    // Boton Continuar del panel de nueva partida que lleva a elegir personaje
    public void OnContinuar()
    {
        MostrarPanel(panelEleccionDePersonaje);
    }

    // Boton del panel de eleccion para elegir al personaje Torrente
    public void OnElegirTorrente()
    {
        CrearPartidaConPersonaje("torrente");
    }

    // Boton del panel de eleccion para elegir al otro personaje
    public void OnElegirPersonaje1()
    {
        CrearPartidaConPersonaje("personaje1");
    }

    // Llama a la API para crear la partida con el nombre y personaje elegidos y arranca el juego
    private void CrearPartidaConPersonaje(string personajeElegido)
    {
        // Leemos el nombre directamente del InputField por si no se disparo el evento
        string nombre;
        if (inputNombrePartida != null && !string.IsNullOrWhiteSpace(inputNombrePartida.text))
        {
            nombre = inputNombrePartida.text;
        }
        else
        {
            nombre = "Mi Partida";
        }

        MostrarCarga("Creando partida...");
        ApiSetup.Partida.CrearPartida(nombre, personajeElegido, ManejarPartidaCreada, ManejarErrorCreacion);
    }

    // Se ejecuta cuando el servidor crea la partida correctamente
    // Inicia la sesion con los datos recibidos y carga la casa del personaje
    private void ManejarPartidaCreada(PartidaCompleta partidaCreada)
    {
        SessionManager.Instance.IniciarConPartida(partidaCreada, new List<IbermonJugador>());
        creadorPersonaje.personajeElegido = partidaCreada.personaje_elegido;
        creadorPersonaje.escenaDestino = "CasaPersonaje";
        creadorPersonaje.crearEInstanciarPersonaje();
    }

    // Se ejecuta si el servidor devuelve error al crear la partida
    private void ManejarErrorCreacion(string mensajeError)
    {
        Debug.LogError($"Error al crear partida: {mensajeError}");
        MostrarPanel(panelEleccionDePersonaje);
    }

    // ── Navegacion ───────────────────────────────────────────────────────────

    // Boton Nueva del panel de partidas que lleva al formulario de nueva partida
    public void OnNuevaPartida()
    {
        MostrarPanel(panelNuevaPartida);
    }

    // Boton Volver que devuelve al jugador al menu principal
    public void OnVolver()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Convierte segundos en un string legible de horas y minutos
    private string FormatearTiempo(int segundos)
    {
        int horas = segundos / 3600;
        int minutos = (segundos % 3600) / 60;
        int segs = segundos % 60;
        return $"{horas}h {minutos}m {segs}s jugadas";
    }

    // Convierte una fecha ISO 8601 del servidor en formato dd/MM/yyyy HH:mm
    // Si la fecha llega vacia o no se puede parsear devuelve un guion
    private string FormatearFecha(string fechaIso)
    {
        if (string.IsNullOrEmpty(fechaIso))
        {
            return "—";
        }

        if (DateTime.TryParse(fechaIso, out DateTime dt))
        {
            // Convertimos de UTC a hora local del dispositivo
            DateTime horaLocal = dt.ToLocalTime();
            return horaLocal.ToString("dd/MM/yyyy HH:mm");
        }

        return fechaIso;
    }

    // Activa solo el panel indicado y desactiva los demas
    private void MostrarPanel(GameObject panelAMostrar)
    {
        panelPartidas.SetActive(panelAMostrar == panelPartidas);
        panelNuevaPartida.SetActive(panelAMostrar == panelNuevaPartida);
        panelEleccionDePersonaje.SetActive(panelAMostrar == panelEleccionDePersonaje);
        panelDetallePartida.SetActive(panelAMostrar == panelDetallePartida);
        panelCarga.SetActive(false);
    }

    // Muestra el panel de carga con un mensaje concreto y oculta los demas
    private void MostrarCarga(string mensaje)
    {
        panelPartidas.SetActive(false);
        panelNuevaPartida.SetActive(false);
        panelEleccionDePersonaje.SetActive(false);
        panelDetallePartida.SetActive(false);
        panelCarga.SetActive(true);

        if (textoCarga != null)
        {
            textoCarga.text = mensaje;
        }
    }
}