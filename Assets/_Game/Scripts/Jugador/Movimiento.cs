using Assets.Scripts.Batalla;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum GameState { FreeRoam, Menu }

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
    MenuController menuController;

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

        menuController = GetComponent<MenuController>();
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

        // Comprobamos que menuController existe antes de suscribirnos
        // El personaje1 puede no tener MenuController en su prefab
        if (menuController != null)
        {
            menuController.onBack += () =>
            {
                state = GameState.FreeRoam;
            };
            menuController.onMenuSelected += OnMenuSelected;
        }
        else
        {
            Debug.LogWarning("[Movimiento] No se encontro MenuController en el personaje, el menu no estara disponible");
        }
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //Pokemons
        }
        else if (selectedItem == 1)
        {
            //Mochila
        }
        else if (selectedItem == 2)
        {
            //Pokedex
        }

        state = GameState.FreeRoam;
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

                // Abrimos el menu con M si el personaje tiene MenuController
                if (Input.GetKeyDown(KeyCode.M))
                {
                    if (menuController != null)
                    {
                        menuController.OpenMenu();
                        state = GameState.Menu;
                    }
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

                // llamada al metodo para detectar abrirMenu de opciones
                if (DetectarMenuOpciones())
                {
                    GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
                    GuardarPosicionAnterior.posicionAnterior = transform.position;

                    // Deshabilitar AudioListener del Player ANTES de cargar la escena
                    var listener = GetComponentInChildren<AudioListener>();
                    if (listener) listener.enabled = false;

                    // Cargamos de manera asincrona para que la musica no se pare
                    SceneManager.LoadSceneAsync("Opciones");
                }
            }
        }
        else if (state == GameState.Menu)
        {
            // Solo procesamos el menu si el personaje tiene MenuController
            if (menuController != null)
            {
                menuController.HandleUpdate();
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

    // --- Método para chequear encuentros en hierba ---
    private void ChequearHierba()
    {
        // Delegamos toda la lógica al PlayerCharacterController
        var controller = GetComponent<PlayerCharacterController>();
        if (controller != null)
            controller.ChequearHierba(transform.position);
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