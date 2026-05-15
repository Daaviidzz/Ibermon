using Assets.Scripts.Batalla;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum GameState { FreeRoam, UIPanel, Bag }

public class Movimiento : MonoBehaviour
{
    //Variable para la velocidad del personaje
    private float velocidad;
    private float velocidadMinima = 3f;
    private float velocidadMaxima = 6f;

    // --- Variables de enfrentamientos en hierba ---
    public LayerMask grassLayer;            // Capa de la hierba
    public float probabilidad = 60f;        // 60% de probabilidad
    private float cronometroPasos;          // Tiempo acumulado caminando
    public float tiempoEntreChequeos = 0.5f;// Cada cuÃ¡nto tiempo tira el dado

    //Variable para hacer referencia al RigidBody
    private Rigidbody2D rigidbody2D;

    //Variable para la animaciÃ³n
    private Animator animacion;

    //creamos una variable tipo vector2 que es la que nos permitirÃ¡ el movimiento
    private Vector2 entradaMovimiento;

    //Para saber si el personaje estÃ¡ en interacciÃ³n o no
    public bool estaEnInteraccion = false;

    GameState state;

    // Referencia al panel de opciones integrado en el prefab del jugador
    private UIOpcionesPanel uiOpcionesPanel;

    // Referencias a las pantallas de partido e inventario
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CentroIbermonUI centroIbermonUI;
    private PokemonParty pokemonParty;

    //Parte movil
    [Header("Controles MÃ³viles (Opcional)")]
    [SerializeField] private JoystickVirtual joystick;            // Referencia al joystick
    [SerializeField] private ActivacionBoton botonCorrer;         // Referencia al botÃ³n de correr
    [SerializeField] private ActivacionBoton botonMenuOpciones;   // Referencia al botÃ³n de menu opciones

    // Detectar si estamos en mÃ³vil o PC
    private bool esMovil;

    //Metodo que se ejecuta nada mÃ¡s iniciar
    private void Awake()
    {
        //Para la parte mÃ³vil
        comprobacionInicialParteMovil();

        //coger el rigidbody del objeto y ponerselo a la variable que hemos creado
        rigidbody2D = GetComponent<Rigidbody2D>();
        //lo mismo pero como la animaciÃ³n no estÃ¡ en el padre y estÃ¡ en el hijo harÃ©
        animacion = GetComponentInChildren<Animator>();

        // Solo bloquear cursor en PC
        if (!esMovil)
        {
            //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
            Cursor.lockState = CursorLockMode.Locked;
            //Y ahora le quitamos la visibilidad
            Cursor.visible = false;
        }

        uiOpcionesPanel = GetComponentInChildren<UIOpcionesPanel>();
        pokemonParty = GetComponent<PokemonParty>();
    }

    // MÃ©todo Start que se ejecuta despuÃ©s del Awake
    private void Start()
    {
        // Al cargar la escena del juego, bloquear controles brevemente
        // Esto evita que inputs de escenas anteriores se cuelen aquÃ­
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.BloquearControlesTemporalmente(0.3f);
        }

        // Si volvemos de Opciones con el panel abierto, restauramos el estado UIPanel
        // El propio UIOpcionesPanel.Start() ya habrÃ¡ reabierto el panel y activado _ignorarInputEsteFrame
        if (UIOpcionesPanel.estaAbierto)
        {
            state = GameState.UIPanel;
        }

        if (partyScreen != null)
        {
            if (CatalogoCache.Instance != null && CatalogoCache.Instance.EstaListo)
            {
                partyScreen.Init(pokemonParty);
            }
            else
            {
                Debug.LogWarning("[Movimiento] CatalogoCache no estÃ¡ listo al iniciar PartyScreen. " +
                    "AsegÃºrate de que ApiSetup se ejecute antes de cargar la escena del juego.");
            }
        }
    }

    //Este mÃ©todo es el que se usa para hacer el movimiento del personaje
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            if (!estaEnInteraccion)
            {
                //llamamos a detectar correr para saber si estÃ¡ corriendo o no
                if (DetectarCorrer())
                {
                    velocidad = velocidadMaxima;
                }
                else
                {
                    velocidad = velocidadMinima;
                }

                //colocamos dentro de nuestro vector2 el x con horizontal y el y con vertical
                entradaMovimiento.x = ObtenerInputHorizontal(); //Horizontal
                entradaMovimiento.y = ObtenerInputVertical();   //Vertical

                //para normalizarlo
                entradaMovimiento = entradaMovimiento.normalized;

                //para que salten las distintas animaciones
                animacion.SetFloat("Horizontal", entradaMovimiento.x);
                animacion.SetFloat("Vertical", entradaMovimiento.y);
                animacion.SetFloat("Velocidad", entradaMovimiento.magnitude);

                // --- Llamada a la funciÃ³n de chequeo de hierba ---
                // Solo chequeamos si el jugador se estÃ¡ moviendo realmente
                if (entradaMovimiento.magnitude > 0.1f)
                {
                    ChequearHierba();
                }

                // Detectar X para abrir el panel de opciones del jugador
                if (DetectarMenuOpciones())
                {
                    if (uiOpcionesPanel != null)
                    {
                        state = GameState.UIPanel;
                        uiOpcionesPanel.AbrirPanel();
                    }
                    else
                    {
                        // Fallback: si no hay UIOpcionesPanel, cargamos la escena como antes
                        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
                        GuardarPosicionAnterior.posicionAnterior = transform.position;

                        var listener = GetComponentInChildren<AudioListener>();
                        if (listener != null)
                        {
                            listener.enabled = false;
                        }

                        SceneManager.LoadSceneAsync("Opciones");
                    }
                }
            }
        }
        else if (state == GameState.Bag)
        {
            // Gestionamos la navegaciÃ³n dentro del inventario
            if (inventoryUI != null)
            {
                inventoryUI.HandleUpdate(onBack: () =>
                {
                    inventoryUI.gameObject.SetActive(false);
                    state = GameState.FreeRoam;
                    if (!esMovil)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                });
            }
        }
    }

    private void FixedUpdate()
    {
        if (state == GameState.FreeRoam && !estaEnInteraccion)
        {
            rigidbody2D.linearVelocity = entradaMovimiento * velocidad;
        }
        else
        {
            rigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    // --- MÃ©todo para chequear encuentros en hierba ---
    private void ChequearHierba()
    {
        // Delegamos toda la lÃ³gica al PlayerCharacterController
        var controller = GetComponent<PlayerCharacterController>();
        if (controller != null)
            controller.ChequearHierba(transform.position);
    }

    // Lo llama UIOpcionesPanel cuando el jugador pulsa Volver
    public void CerrarUIPanel()
    {
        state = GameState.FreeRoam;
        if (!esMovil)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void AbrirMochila()
    {
        if (inventoryUI != null)
        {
            Debug.Log("[Movimiento] Abriendo mochila.");
            inventoryUI.gameObject.SetActive(true);
            inventoryUI.Abrir();
            state = GameState.Bag;
        }
        else
        {
            Debug.LogWarning("[Movimiento] No hay InventoryUI asignado.");
        }
    }

    // Abre la pantalla de gestion de pokemon en overworld
    public void AbrirCentroIbermon()
    {
        if (!CentroIbermonTieneCanvasPropio())
        {
            if (centroIbermonUI != null)
            {
                centroIbermonUI.gameObject.SetActive(false);
            }

            var prefab = Resources.Load<CentroIbermonUI>("UI/CentroIbermonCanvas");
            if (prefab == null)
            {
                Debug.LogError("[Movimiento] No se encontro Resources/UI/CentroIbermonCanvas.");
                return;
            }

            centroIbermonUI = Instantiate(prefab);
        }

        centroIbermonUI.gameObject.SetActive(true);
        AjustarEscaladoCentroIbermon();
        centroIbermonUI.Abrir();
        state = GameState.UIPanel;
    }

    private bool CentroIbermonTieneCanvasPropio()
    {
        return centroIbermonUI != null
            && centroIbermonUI.transform.parent == null
            && centroIbermonUI.GetComponent<Canvas>() != null
            && centroIbermonUI.GetComponent<CanvasScaler>() != null;
    }

    private void AjustarEscaladoCentroIbermon()
    {
        var scaler = centroIbermonUI.GetComponent<CanvasScaler>();
        if (scaler == null) return;

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1100f, 620f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    // ========== FUNCIONES MULTIPLATAFORMA ==========

    //Parte movil inicial
    private void comprobacionInicialParteMovil()
    {
        // Detectar la plataforma
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif

        // Desactivar controles en PC
        if (!esMovil && ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.gameObject.SetActive(false);
        }
    }

    // Obtener input horizontal (A/D o Joystick)
    float ObtenerInputHorizontal()
    {
        if (esMovil && ControlesMoviles.Instance.joystick != null)
        {
            // Usar joystick en mÃ³vil
            return ControlesMoviles.Instance.joystick.Horizontal();
        }
        else
        {
            // Usar teclado en PC
            return Input.GetAxis("Horizontal");
        }
    }

    // Obtener input vertical (W/S o Joystick)
    float ObtenerInputVertical()
    {
        if (esMovil && ControlesMoviles.Instance.joystick != null)
        {
            // Usar joystick en mÃ³vil
            return ControlesMoviles.Instance.joystick.Vertical();
        }
        else
        {
            // Usar teclado en PC
            return Input.GetAxis("Vertical");
        }
    }

    // Detectar si se presionÃ³ el botÃ³n de correr
    bool DetectarCorrer()
    {
        if (esMovil && ControlesMoviles.Instance.botonCorrer != null)
        {
            // Usar botÃ³n tÃ¡ctil en mÃ³vil
            return ControlesMoviles.Instance.botonCorrer.EstaPresionado();
        }
        else
        {
            // Usar tecla espacio en PC
            return Input.GetKey(KeyCode.LeftShift);
        }
    }

    // Detectar si se presionÃ³ el botÃ³n de abrir menu opciones
    bool DetectarMenuOpciones()
    {
        if (esMovil && ControlesMoviles.Instance.botonMenuOpciones != null)
        {
            // Usar botÃ³n tÃ¡ctil en mÃ³vil
            return ControlesMoviles.Instance.botonMenuOpciones.SePresionoEsteFrame();
        }
        else
        {
            // Usar tecla X en PC
            return Input.GetKeyDown(KeyCode.X);
        }
    }
}
