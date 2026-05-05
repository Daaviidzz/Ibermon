using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

// Controlador del profesor que gestiona la eleccion del starter y el dialogo inicial
// Se coloca en el mismo GameObject que tiene el componente Interactuable del profesor
// Flujo:
//   1. Al interactuar comprueba si el jugador ya tiene starter en la sesion
//   2. Si no tiene starter muestra el panel de eleccion y espera a que pulse un boton
//   3. Cuando elige llama a la API para guardar el starter
//   4. Pide el equipo a la API y lo carga en el PokemonParty del jugador
//   5. Lanza el dialogo normal del profesor
public class ProfesorController : MonoBehaviour
{
    // Canvas con el panel de eleccion de starter, asignar desde el inspector
    public GameObject canvasEleccionStarter;

    // Referencia al componente Interactuable del mismo GameObject o de un hijo
    // Lo cogemos automaticamente en el Awake
    private Interactuable _interactuable;

    // Guardia para evitar que OnInteraccion se llame varias veces seguidas
    // mientras se esta procesando una interaccion anterior
    private bool _interaccionEnCurso = false;

    private void Awake()
    {
        _interactuable = GetComponent<Interactuable>();
        if (_interactuable == null)
        {
            _interactuable = GetComponentInChildren<Interactuable>();
        }
    }

    private void Start()
    {
        // Ocultamos el canvas de eleccion al arrancar por si estuviera visible en el editor
        if (canvasEleccionStarter != null)
        {
            canvasEleccionStarter.SetActive(false);
        }
    }

    // Llamado desde Interactuable cuando el jugador interactua con el profesor
    // Decide si mostrar el panel de starter o ir directo al dialogo
    // Usamos > 0 como comprobacion porque JsonUtility convierte null a 0
    // y el backend devuelve 0 cuando no hay starter elegido todavia
    public void OnInteraccion()
    {
        // Evitamos que se llame varias veces seguidas mientras procesamos
        if (_interaccionEnCurso)
        {
            return;
        }
        _interaccionEnCurso = true;

        Debug.Log("[ProfesorController] OnInteraccion llamado");

        if (SessionManager.Instance == null)
        {
            Debug.LogError("[ProfesorController] SessionManager.Instance es null");
            _interaccionEnCurso = false;
            return;
        }

        if (SessionManager.Instance.PartidaActual == null)
        {
            Debug.LogError("[ProfesorController] PartidaActual es null");
            _interaccionEnCurso = false;
            return;
        }

        Debug.Log($"[ProfesorController] starter_elegido = {SessionManager.Instance.PartidaActual.starter_elegido}");

        bool tieneStarter = SessionManager.Instance.PartidaActual.starter_elegido > 0;

        Debug.Log($"[ProfesorController] tieneStarter = {tieneStarter}");

        if (tieneStarter)
        {
            // Ya tiene starter, lanzamos el dialogo directamente
            _interaccionEnCurso = false;
            _interactuable.IniciarDialogoDesdeEntrenador();
        }
        else
        {
            // No tiene starter, mostramos el panel de eleccion
            // Bloqueamos el movimiento del jugador mientras elige
            var movimiento = GameObject.FindWithTag("Player")?.GetComponent<Movimiento>();
            if (movimiento != null)
            {
                movimiento.estaEnInteraccion = true;
            }

            // Activamos el cursor para que el jugador pueda clicar en los botones del starter
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            canvasEleccionStarter.SetActive(true);
        }
    }

    // Boton starter 1 del panel de eleccion
    public void OnElegirStarter1()
    {
        ElegirStarter(1);
    }

    // Boton starter 2 del panel de eleccion
    public void OnElegirStarter2()
    {
        ElegirStarter(2);
    }

    // Boton starter 3 del panel de eleccion
    public void OnElegirStarter3()
    {
        ElegirStarter(3);
    }

    // Llama a la API para guardar el starter elegido y cierra el panel
    private void ElegirStarter(int numeroStarter)
    {
        string partidaId = SessionManager.Instance.PartidaId;
        ApiSetup.Partida.ElegirStarter(partidaId, numeroStarter,
            ManejarStarterElegidoExitoso, ManejarErrorStarter);
    }

    // Se ejecuta cuando la API confirma que el starter se guardo correctamente
    // Cierra el panel y pide el equipo actualizado a la API
    private void ManejarStarterElegidoExitoso(PartidaCompleta partidaActualizada)
    {
        // Actualizamos la partida en sesion con el starter ya asignado
        SessionManager.Instance.IniciarConPartida(partidaActualizada,
            new List<IbermonJugador>(SessionManager.Instance.EquipoAPI));

        canvasEleccionStarter.SetActive(false);

        // Volvemos a bloquear el cursor al cerrar el panel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Pedimos el equipo a la API para que meta el starter recien creado en el PokemonParty
        ApiSetup.IbermonJugador.ObtenerEquipo(partidaActualizada.id,
            ManejarEquipoRecibido, ManejarErrorEquipo);
    }

    // Se ejecuta cuando llega el equipo de la API tras elegir starter
    // Lo guarda en sesion, hace que el PokemonParty del jugador se recargue y lanza el dialogo
    private void ManejarEquipoRecibido(List<IbermonJugador> equipo)
    {
        SessionManager.Instance.SetEquipoAPI(equipo);

        // Forzamos al PokemonParty del jugador a recargar el equipo desde la sesion
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var party = player.GetComponent<PokemonParty>();
            if (party != null)
            {
                party.CargarEquipoGuardado();
            }
        }

        _interaccionEnCurso = false;

        if (_interactuable != null)
        {
            _interactuable.IniciarDialogoDesdeEntrenador();
        }
        else
        {
            Debug.LogError("[ProfesorController] No se encontro Interactuable, desbloqueando jugador manualmente");
            var mov = GameObject.FindWithTag("Player")?.GetComponent<Movimiento>();
            if (mov != null)
            {
                mov.estaEnInteraccion = false;
            }
        }
    }

    // Se ejecuta si no se pudo obtener el equipo de la API
    // Lanzamos el dialogo igualmente para no bloquear al jugador
    private void ManejarErrorEquipo(string mensajeError)
    {
        Debug.LogError($"[ProfesorController] Error al obtener equipo: {mensajeError}");
        _interaccionEnCurso = false;
        _interactuable.IniciarDialogoDesdeEntrenador();
    }

    // Se ejecuta si la API devuelve error al guardar el starter
    private void ManejarErrorStarter(string mensajeError)
    {
        Debug.LogError($"[ProfesorController] Error al guardar starter: {mensajeError}");

        canvasEleccionStarter.SetActive(false);

        // Volvemos a bloquear el cursor al cerrar el panel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _interaccionEnCurso = false;

        var movimiento = GameObject.FindWithTag("Player")?.GetComponent<Movimiento>();
        if (movimiento != null)
        {
            movimiento.estaEnInteraccion = false;
        }
    }
}