using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controlador de la escena "Partidas".
/// Lista las partidas del usuario desde la API y permite crear una nueva o cargar una existente.
///
/// ESTRUCTURA DE UI necesaria en la escena "Partidas":
/// ─────────────────────────────────────────────────────
///  Canvas
///  ├── PanelListaPartidas
///  │   ├── ScrollView → Viewport → Content   ← asignar a "contenedorPartidas"
///  │   ├── Button "Nueva Partida"             ← asignar a "botonNueva"
///  │   └── Button "Volver"                   ← asignar a "botonVolver"
///  │
///  ├── PanelNuevaPartida
///  │   ├── Dropdown "Personaje" (Chico / Chica) ← asignar a "dropdownPersonaje"
///  │   ├── Dropdown "Starter"  (3 opciones)     ← asignar a "dropdownStarter"
///  │   ├── Button "Confirmar"                   ← asignar a "botonConfirmarNueva"
///  │   └── Button "Cancelar"                    ← asignar a "botonCancelarNueva"
///  │
///  └── PanelCargando
///      └── TextMeshProUGUI                      ← asignar a "textoCargando"
///
///  Prefab "PartidaEntry" con el componente PartidaEntryUI ← asignar a "prefabEntradaPartida"
///  CrearYPosicionarPlayer en escena              ← asignar a "posicionarPlayer"
/// </summary>
public class MenuPartida : MonoBehaviour
{
    // ─── Referencias UI ───────────────────────────────────────────────────────

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
    public GameObject    panelCargando;
    public TextMeshProUGUI textoCargando;

    [Header("Prefab")]
    [Tooltip("Prefab con el componente PartidaEntryUI")]
    public GameObject    prefabEntradaPartida;

    [Header("Escenas")]
    public string        escenaJuego = "PuebloFuenlabrada";

    [Header("Referencia Player")]
    [Tooltip("GameObject de la escena que crea y posiciona al jugador")]
    public CrearYPosicionarPlayer posicionarPlayer;

    // ─── Starters disponibles ─────────────────────────────────────────────────
    // Mapeados por índice del dropdown:
    //   0 → Ignifor (catalogoId=1)
    //   1 → Aquillo (catalogoId=4)
    //   2 → Verdino (catalogoId=7)
    // Ajusta los IDs si tu seed de la API usa otros números.
    private readonly int[] _starterCatalogoIds = { 1, 4, 7 };

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        InicializarDropdowns();
        MostrarCargando("Cargando partidas...");
        CargarListaPartidas();
    }

    // ─── Lista de partidas ────────────────────────────────────────────────────

    private void CargarListaPartidas()
    {
        ApiSetup.Partida.ListarPartidas(
            lista =>
            {
                LimpiarContenedor();
                if (lista.Count == 0)
                {
                    // Sin partidas: mostrar directamente el panel de nueva partida
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
                // Si el token expiró, volver al login
                if (err.Contains("[401]"))
                {
                    ApiSetup.Auth.Logout();
                    SceneManager.LoadScene("Login");
                    return;
                }
                MostrarPanelLista(); // Mostrar lista vacía con el botón Nueva
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

    // ─── Cargar partida existente ─────────────────────────────────────────────

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
                        // Iniciar sesión con los datos cargados
                        SessionManager.Instance.IniciarConPartida(partida, equipo);

                        // Posicionar al jugador en las coordenadas guardadas
                        JugadorSpawn.posicion = new Vector2(partida.posicion.x, partida.posicion.y);

                        // Crear jugador y cargar escena del juego
                        IniciarJuego(partida.mapa_actual);
                    },
                    err => ManejarErrorCarga(err));
            },
            err => ManejarErrorCarga(err));
    }

    // ─── Nueva partida ────────────────────────────────────────────────────────

    public void MostrarPanelNueva()
    {
        panelLista.SetActive(false);
        panelNuevaPartida.SetActive(true);
        panelCargando.SetActive(false);
    }

    /// <summary>Conectado al botón "Confirmar" del panel nueva partida.</summary>
    public void OnClickConfirmarNueva()
    {
        string personaje  = dropdownPersonaje.value == 0 ? "chico" : "chica";
        int    starterIdx = Mathf.Clamp(dropdownStarter.value, 0, _starterCatalogoIds.Length - 1);
        int    starterId  = _starterCatalogoIds[starterIdx];

        MostrarCargando("Creando partida...");

        ApiSetup.Partida.CrearPartida(personaje, starterId,
            partida =>
            {
                // Partida nueva: equipo vacío (el starter lo crea la API internamente)
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
                        // Si no tiene equipo aún, iniciar con lista vacía igualmente
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

    /// <summary>Conectado al botón "Cancelar" del panel nueva partida.</summary>
    public void OnClickCancelarNueva()
    {
        MostrarCargando("Cargando partidas...");
        CargarListaPartidas();
    }

    // ─── Eliminar partida ─────────────────────────────────────────────────────

    private void EliminarPartida(string partidaId, GameObject entrada)
    {
        // Aquí podrías mostrar un diálogo de confirmación antes de eliminar
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

    // ─── Botones ──────────────────────────────────────────────────────────────

    public void BotonNueva()  => MostrarPanelNueva();
    public void BotonVolver() => SceneManager.LoadScene("MenuPrincipal");

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private void IniciarJuego(string escena)
    {
        string nombreEscena = string.IsNullOrEmpty(escena) ? escenaJuego : escena;

        if (posicionarPlayer != null)
            posicionarPlayer.crearEInstanciarPersonaje();
        else
            SceneManager.LoadScene(nombreEscena);
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
