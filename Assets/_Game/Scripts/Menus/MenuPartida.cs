using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla la escena "Partidas": lista las partidas del usuario, deja crear una nueva
// o cargar una existente.
// Ver MANUAL_ESCENAS_UNITY.md → Sección "ESCENA 3 — Partidas" para la estructura de UI.
public class MenuPartida : MonoBehaviour
{
    [Header("Panel Lista")]
    public GameObject    panelLista;
    public Transform     contenedorPartidas;
    public Button        botonNueva;
    public Button        botonVolver;

    [Header("Panel Nueva Partida")]
    public GameObject    panelNuevaPartida;
    public TMP_Dropdown  dropdownPersonaje;
    public TMP_Dropdown  dropdownStarter;
    public Button        botonConfirmarNueva;
    public Button        botonCancelarNueva;

    [Header("Panel Cargando")]
    public GameObject      panelCargando;
    public TextMeshProUGUI textoCargando;

    [Header("Prefab")]
    [Tooltip("Prefab con el componente PartidaEntryUI")]
    public GameObject prefabEntradaPartida;

    [Header("Escenas")]
    public string escenaJuego = "PuebloFuenlabrada";

    [Header("Referencia Player")]
    [Tooltip("El SpawnHelper de la escena — solo se usa para partidas nuevas")]
    public CrearYPosicionarPlayer posicionarPlayer;

    // IDs del catálogo de starters — ajustar si el seed de la API usa otros números
    // 0 → Ignifor (1), 1 → Aquillo (4), 2 → Verdino (7)
    private readonly int[] _starterCatalogoIds = { 1, 4, 7 };

    private void Start()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        InicializarDropdowns();
        MostrarCargando("Cargando partidas...");
        CargarListaPartidas();
    }

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
        entry.Inicializar(
            resumen,
            onSeleccionar: r => CargarPartida(r.id),
            onEliminar:    r => EliminarPartida(r.id, go)
        );
    }

    private void LimpiarContenedor()
    {
        foreach (Transform child in contenedorPartidas)
            Destroy(child.gameObject);
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

    public void MostrarPanelNueva()
    {
        panelLista.SetActive(false);
        panelNuevaPartida.SetActive(true);
        panelCargando.SetActive(false);
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

    // Conectado al botón "Cancelar" del panel nueva partida
    public void OnClickCancelarNueva()
    {
        MostrarCargando("Cargando partidas...");
        CargarListaPartidas();
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

    public void BotonNueva()  => MostrarPanelNueva();
    public void BotonVolver() => SceneManager.LoadScene("MenuPrincipal");

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

    private void MostrarCargando(string mensaje)
    {
        panelLista.SetActive(false);
        panelNuevaPartida.SetActive(false);
        panelCargando.SetActive(true);
        if (textoCargando) textoCargando.text = mensaje;
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
