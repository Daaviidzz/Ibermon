using UnityEngine;
using UnityEngine.SceneManagement;

public class JugadorSpawn : MonoBehaviour
{
    public static Vector2 posicion       = Vector2.zero;
    public static string  escenaAnterior = "";

    // Ponlo a true antes de LoadScene cuando cargues una partida guardada.
    // Así el jugador aparece en la posición exacta aunque sea (0,0).
    // Se resetea solo después de usarse.
    public static bool usarPosicionGuardada = false;

    private Movimiento scriptMovimiento;
    private Renderer[] renderizadores;
    private Collider2D colisionador;
    private Camera cameraPlayer;
    private AudioListener audioListenerPlayer;

    private void Awake()
    {
        if (GameObject.FindWithTag("Player") != null &&
            GameObject.FindWithTag("Player") != gameObject)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        scriptMovimiento    = GetComponent<Movimiento>();
        renderizadores      = GetComponentsInChildren<Renderer>();
        colisionador        = GetComponent<Collider2D>();
        cameraPlayer        = GetComponentInChildren<Camera>();
        audioListenerPlayer = GetComponentInChildren<AudioListener>();
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // Escenas donde el jugador debe estar completamente oculto e inactivo.
    // Si añades una escena de menú nueva, ponla aquí.
    private static readonly System.Collections.Generic.HashSet<string> _escenasOcultas =
        new System.Collections.Generic.HashSet<string>
        {
            "Portada", "Login", "MenuPrincipal", "Partidas", "Creditos", "Opciones", "Combate"
        };

    private void OcultarJugador()
    {
        if (scriptMovimiento)    scriptMovimiento.enabled    = false;
        foreach (var r in renderizadores) r.enabled          = false;
        if (colisionador)        colisionador.enabled        = false;
        if (cameraPlayer)        cameraPlayer.enabled        = false;
        if (audioListenerPlayer) audioListenerPlayer.enabled = false;
    }

    private void MostrarJugador()
    {
        if (scriptMovimiento)    scriptMovimiento.enabled    = true;
        foreach (var r in renderizadores) r.enabled          = true;
        if (colisionador)        colisionador.enabled        = true;
        if (cameraPlayer)        cameraPlayer.enabled        = true;
        if (audioListenerPlayer) audioListenerPlayer.enabled = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Escenas de menú: el jugador no debe verse ni moverse
        if (_escenasOcultas.Contains(scene.name))
        {
            OcultarJugador();
            return;
        }

        // Mapa del juego: activar el jugador y posicionarlo
        MostrarJugador();

        // Cargando desde partida guardada — posición exacta de la API
        if (usarPosicionGuardada)
        {
            transform.position  = posicion;
            usarPosicionGuardada = false;
            return;
        }

        // Volviendo desde Opciones — restaurar donde estaba
        if (GuardarPosicionAnterior.escenaAnterior != "" &&
            scene.name == GuardarPosicionAnterior.escenaAnterior)
        {
            transform.position = GuardarPosicionAnterior.posicionAnterior;
            GuardarPosicionAnterior.escenaAnterior = "";
            return;
        }

        // Transición normal entre mapas
        if (posicion != Vector2.zero)
            transform.position = posicion;
    }
}
