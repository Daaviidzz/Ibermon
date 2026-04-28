using ApiRest.Managers;
using ApiRest.Models;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIOpcionesPanel : MonoBehaviour
{
    // --- Bool estático para que otras escenas sepan si el panel está abierto ---
    public static bool estaAbierto = false;

    // --- Referencias al panel raíz ---
    public GameObject panel;

    // --- Textos del panel izquierdo ---
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoTiempoJugado;
    public TextMeshProUGUI textoFechaCreacion;
    public TextMeshProUGUI textoUltimaConexion;

    // --- Botones del panel derecho ---
    public Button botonPokemons;
    public Button botonMochila;
    public Button botonOpciones;
    public Button botonGuardar;
    public Button botonVolver;

    // --- Texto de confirmación de guardado ---
    public GameObject textoGuardadoOk;

    // --- Referencia al Movimiento para devolver el control al cerrar ---
    private Movimiento movimiento;

    // --- Detectar plataforma ---
    private bool esMovil;

    private bool _ignorarInputEsteFrame = false;

    private void Awake()
    {
        ComprobacionInicialParteMovil();

        movimiento = GetComponent<Movimiento>();

        botonPokemons.onClick.AddListener(AbrirPokemons);
        botonMochila.onClick.AddListener(AbrirMochila);
        botonOpciones.onClick.AddListener(AbrirOpciones);
        botonGuardar.onClick.AddListener(GuardarPartida);
        botonVolver.onClick.AddListener(CerrarPanel);

        // El panel empieza cerrado
        panel.SetActive(false);

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }
    }

    private void Update()
    {
        // Si acabamos de abrir el panel este frame, ignoramos el input para no cerrarlo al instante
        if (_ignorarInputEsteFrame)
        {
            _ignorarInputEsteFrame = false;
            return;
        }

        if (panel.activeSelf && Input.GetKeyDown(KeyCode.X))
        {
            CerrarPanel();
        }
    }

    // Abre el panel, muestra los datos de la partida y desbloquea el cursor
    public void AbrirPanel()
    {
        _ignorarInputEsteFrame = true;
        estaAbierto = true;
        ActualizarDatosPartida();
        panel.SetActive(true);

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }
        // Solo en PC desbloqueamos el cursor para poder pulsar los botones
        if (!esMovil)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Cierra el panel y devuelve el control al jugador
    public void CerrarPanel()
    {
        estaAbierto = false;
        panel.SetActive(false);

        // Solo en PC volvemos a bloquear el cursor
        if (!esMovil)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Avisamos a Movimiento para que vuelva al estado FreeRoam
        if (movimiento != null)
        {
            movimiento.CerrarUIPanel();
        }
    }

    // Rellena los textos con los datos de la partida activa en SessionManager
    private void ActualizarDatosPartida()
    {
        if (!SessionManager.Instance.TienePartida)
        {
            Debug.LogWarning("[UIOpcionesPanel] No hay partida activa para mostrar datos");
            return;
        }

        PartidaCompleta partida = SessionManager.Instance.PartidaActual;

        if (textoNombre != null)
        {
            if (!string.IsNullOrEmpty(partida.nombre))
            {
                textoNombre.text = partida.nombre;
            }
            else
            {
                Debug.LogWarning("[UIOpcionesPanel] El nombre de la partida esta vacio o es null");
                textoNombre.text = "-";
            }
        }

        if (textoTiempoJugado != null)
        {
            int segundos = SessionManager.Instance.TiempoJugadoSegundos;
            int horas = segundos / 3600;
            int minutos = (segundos % 3600) / 60;
            int segs = segundos % 60;
            textoTiempoJugado.text = $"{horas}h {minutos}m {segs}s";
        }

        if (textoFechaCreacion != null)
        {
            if (DateTime.TryParse(partida.fecha_creacion, out DateTime fechaCreacion))
            {
                textoFechaCreacion.text = fechaCreacion.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                textoFechaCreacion.text = partida.fecha_creacion;
            }
        }

        if (textoUltimaConexion != null)
        {
            if (DateTime.TryParse(partida.ultima_conexion, out DateTime ultimaConexion))
            {
                textoUltimaConexion.text = ultimaConexion.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
            }
            else
            {
                textoUltimaConexion.text = partida.ultima_conexion;
            }
        }
    }

    // Sin funcionalidad por ahora
    private void AbrirPokemons()
    {
        Debug.Log("[UIOpcionesPanel] Pokemons: sin implementar");
    }

    // Sin funcionalidad por ahora
    private void AbrirMochila()
    {
        Debug.Log("[UIOpcionesPanel] Mochila: sin implementar");
    }

    // Carga la escena Opciones igual que antes, guardando posición y escena actuales
    private void AbrirOpciones()
    {
        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
        GuardarPosicionAnterior.posicionAnterior = transform.position;

        // Deshabilitar AudioListener del Player ANTES de cargar la escena
        var listener = GetComponentInChildren<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }

        SceneManager.LoadSceneAsync("Opciones");
    }

    // Envía la posición y tiempo actuales del jugador a la API
    private void GuardarPartida()
    {
        if (!SessionManager.Instance.TienePartida)
        {
            Debug.LogWarning("[UIOpcionesPanel] No hay partida activa, no se puede guardar");
            return;
        }

        string escena = SceneManager.GetActiveScene().name;
        float x = transform.position.x;
        float y = transform.position.y;
        string partidaId = SessionManager.Instance.PartidaId;
        int tiempoJugado = SessionManager.Instance.TiempoJugadoSegundos;
        string ultimaConexion = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        ApiSetup.Partida.ActualizarPosicion(partidaId, escena, x, y, tiempoJugado, ultimaConexion,
            ManejarGuardadoExitoso, ManejarGuardadoFallido);
    }

    // Se ejecuta cuando la API confirma que se guardó correctamente
    private void ManejarGuardadoExitoso(PartidaCompleta partidaActualizada)
    {
        Debug.Log("[UIOpcionesPanel] Partida guardada correctamente");

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(true);
            StartCoroutine(OcultarTextoGuardado());
        }
    }

    // Oculta el texto de confirmación tras 4 segundos
    private IEnumerator OcultarTextoGuardado()
    {
        yield return new WaitForSeconds(4f);

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }
    }

    // Se ejecuta si la API devuelve error al guardar
    private void ManejarGuardadoFallido(string mensajeError)
    {
        Debug.LogError($"[UIOpcionesPanel] Error al guardar partida: {mensajeError}");
    }

    private void ComprobacionInicialParteMovil()
    {
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif
    }
}