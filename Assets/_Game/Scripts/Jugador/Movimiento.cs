using Assets.Scripts.Batalla;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum GameState { FreeRoam, UIPanel, PartyScreen, Bag }

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
    public float tiempoEntreChequeos = 0.5f;// Cada cuánto tiempo tira el dado

    //Variable para hacer referencia al RigidBody
    private Rigidbody2D rigidbody2D;

    //Variable para la animación
    private Animator animacion;

    //creamos una variable tipo vector2 que es la que nos permitirá el movimiento
    private Vector2 entradaMovimiento;

    //Para saber si el personaje está en interacción o no
    public bool estaEnInteraccion = false;

    GameState state;

    // Referencia al panel de opciones integrado en el prefab del jugador
    private UIOpcionesPanel uiOpcionesPanel;

    // Referencias a las pantallas de partido e inventario
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private InventoryUI inventoryUI;
    private PokemonParty pokemonParty;

    //Parte movil
    [Header("Controles Móviles (Opcional)")]
    [SerializeField] private JoystickVirtual joystick;            // Referencia al joystick
    [SerializeField] private ActivacionBoton botonCorrer;         // Referencia al botón de correr
    [SerializeField] private ActivacionBoton botonMenuOpciones;   // Referencia al botón de menu opciones

    // Detectar si estamos en móvil o PC
    private bool esMovil;

    //Metodo que se ejecuta nada más iniciar
    private void Awake()
    {
        //Para la parte móvil
        comprobacionInicialParteMovil();

        //coger el rigidbody del objeto y ponerselo a la variable que hemos creado
        rigidbody2D = GetComponent<Rigidbody2D>();
        //lo mismo pero como la animación no está en el padre y está en el hijo haré
        animacion = GetComponentInChildren<Animator>();

        // Solo bloquear cursor en PC
        if (!esMovil)
        {
            //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
            Cursor.lockState = CursorLockMode.Locked;
            //Y ahora le quitamos la visibilidad
            Cursor.visible = false;
        }

        uiOpcionesPanel = GetComponent<UIOpcionesPanel>();
        pokemonParty = GetComponent<PokemonParty>();
    }

    // Método Start que se ejecuta después del Awake
    private void Start()
    {
        // Al cargar la escena del juego, bloquear controles brevemente
        // Esto evita que inputs de escenas anteriores se cuelen aquí
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.BloquearControlesTemporalmente(0.3f);
        }

        // Inicializamos la pantalla de equipo con el partido actual
        if (partyScreen != null && pokemonParty != null)
        {
            partyScreen.Init(pokemonParty);
        }

        // Si volvemos de Opciones con el panel abierto, restauramos el estado UIPanel
        // El propio UIOpcionesPanel.Start() ya habrá reabierto el panel y activado _ignorarInputEsteFrame
        if (UIOpcionesPanel.estaAbierto)
        {
            state = GameState.UIPanel;
        }
    }

    //Este método es el que se usa para hacer el movimiento del personaje
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            if (!estaEnInteraccion)
            {
                //llamamos a detectar correr para saber si está corriendo o no
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

                // --- Llamada a la función de chequeo de hierba ---
                // Solo chequeamos si el jugador se está moviendo realmente
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
        else if (state == GameState.PartyScreen)
        {
            // Gestionamos la navegación dentro de la pantalla de equipo
            if (partyScreen != null)
            {
                partyScreen.HandleUpdate(
                    onSelected: () => { },  // de momento vacío
                    onBack: () =>
                    {
                        partyScreen.gameObject.SetActive(false);
                        state = GameState.FreeRoam;
                    }
                );
            }
        }
        else if (state == GameState.Bag)
        {
            // Gestionamos la navegación dentro del inventario
            if (inventoryUI != null)
            {
                inventoryUI.HandleUpdate(onBack: () =>
                {
                    inventoryUI.gameObject.SetActive(false);
                    state = GameState.FreeRoam;
                });
            }
        }
        // En estado UIPanel no procesamos ningún input de movimiento ni de menú
        // El panel se gestiona íntegramente desde UIOpcionesPanel
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

    // --- Método para chequear encuentros en hierba ---
    private void ChequearHierba()
    {
        // Delegamos toda la lógica al PlayerCharacterController
        var controller = GetComponent<PlayerCharacterController>();
        if (controller != null)
            controller.ChequearHierba(transform.position);
    }

    // Lo llama UIOpcionesPanel cuando el jugador pulsa Volver
    public void CerrarUIPanel()
    {
        state = GameState.FreeRoam;
    }

    // Lo llama UIOpcionesPanel cuando el jugador pulsa el botón Pokemons
    public void AbrirPartyScreen()
    {
        if (partyScreen != null)
        {
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
    }

    // Lo llama UIOpcionesPanel cuando el jugador pulsa el botón Mochila
    public void AbrirMochila()
    {
        if (inventoryUI != null)
        {
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
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
            // Usar joystick en móvil
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
            // Usar joystick en móvil
            return ControlesMoviles.Instance.joystick.Vertical();
        }
        else
        {
            // Usar teclado en PC
            return Input.GetAxis("Vertical");
        }
    }

    // Detectar si se presionó el botón de correr
    bool DetectarCorrer()
    {
        if (esMovil && ControlesMoviles.Instance.botonCorrer != null)
        {
            // Usar botón táctil en móvil
            return ControlesMoviles.Instance.botonCorrer.EstaPresionado();
        }
        else
        {
            // Usar tecla espacio en PC
            return Input.GetKey(KeyCode.LeftShift);
        }
    }

    // Detectar si se presionó el botón de abrir menu opciones
    bool DetectarMenuOpciones()
    {
        if (esMovil && ControlesMoviles.Instance.botonMenuOpciones != null)
        {
            // Usar botón táctil en móvil
            return ControlesMoviles.Instance.botonMenuOpciones.SePresionoEsteFrame();
        }
        else
        {
            // Usar tecla X en PC
            return Input.GetKeyDown(KeyCode.X);
        }
    }
}