using UnityEngine;
using UnityEngine.SceneManagement;

public class JugadorSpawn : MonoBehaviour
{
    public static Vector2 posicion = Vector2.zero;
    public static string escenaAnterior = "";

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

        scriptMovimiento = GetComponent<Movimiento>();
        renderizadores = GetComponentsInChildren<Renderer>();
        colisionador = GetComponent<Collider2D>();
        cameraPlayer = GetComponentInChildren<Camera>();
        audioListenerPlayer = GetComponentInChildren<AudioListener>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // CASO 1: Escenas donde el Player no debe estar visible ni activo
        if (scene.name == "Combate" || scene.name == "Opciones")
        {
            if (scriptMovimiento) scriptMovimiento.enabled = false;
            foreach (var r in renderizadores) r.enabled = false;
            if (colisionador) colisionador.enabled = false;
            if (cameraPlayer) cameraPlayer.enabled = false;
            if (audioListenerPlayer) audioListenerPlayer.enabled = false;
        }
        // CASO 2: Mapas del juego
        else
        {
            if (scriptMovimiento) scriptMovimiento.enabled = true;
            foreach (var r in renderizadores) r.enabled = true;
            if (colisionador) colisionador.enabled = true;
            if (cameraPlayer) cameraPlayer.enabled = true;
            if (audioListenerPlayer) audioListenerPlayer.enabled = true;

            // Restaurar posición si venimos de Opciones
            if (GuardarPosicionAnterior.escenaAnterior != "" &&
                 scene.name == GuardarPosicionAnterior.escenaAnterior)
            {
                transform.position = GuardarPosicionAnterior.posicionAnterior;
                GuardarPosicionAnterior.escenaAnterior = "";
                return;
            }

            if (posicion != Vector2.zero)
            {
                transform.position = posicion;
            }
        }
    }
}