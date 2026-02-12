using Assets.Scripts.Batalla;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Movimiento : MonoBehaviour
{
    //Variable para la velocidad del personaje
    private float velocidad;
    private float velocidadMinima = 3f;
    private float velocidadMaxima = 6f;

    // --- Variables de enfrentamientos en hierba ---
    public LayerMask grassLayer;         // Capa de la hierba
    public float probabilidad = 60f;    // 60% de probabilidad
    private float cronometroPasos;      // Tiempo acumulado caminando
    public float tiempoEntreChequeos = 0.5f; // Cada cuánto tiempo tira el dado

    //Variable para hacer referencia al RigidBody
    private Rigidbody2D rigidbody2D;

    //Variable para la animación
    private Animator animacion; // Cambiado a Animator

    //creamos una variable tipo vector2 que es la que nos permitirá el movimiento
    private Vector2 entradaMovimiento;

    //Para saber si el personaje está en interacción o no
    public bool estaEnInteraccion = false;


    //Parte movil
    [Header("Controles Móviles (Opcional)")]
    [SerializeField] private JoystickVirtual joystick; // Referencia al joystick
    [SerializeField] private ActivacionBoton botonCorrer; // Referencia al botón de correr
    [SerializeField] private ActivacionBoton botonMenuOpciones; // Referencia al botón de correr

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

    }

    // Añade este método Start después del Awake
    private void Start()
    {
        // Al cargar la escena del juego, bloquear controles brevemente
        // Esto evita que inputs de escenas anteriores se cuelen aquí
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.BloquearControlesTemporalmente(0.3f);
        }
    }

    //Este método es el que se usa para hacer el movimiento del personaje
    private void Update()
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
            entradaMovimiento.x = ObtenerInputHorizontal();//Horizontal   
            entradaMovimiento.y = ObtenerInputVertical();//Vertical

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

            // llamada al metodo para detectar abrirMenu
            if (DetectarMenuOpciones())
            {
                GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
                GuardarPosicionAnterior.posicionAnterior = transform.position;

                // Deshabilitar AudioListener del Player ANTES de cargar la escena
                var listener = GetComponentInChildren<AudioListener>();
                if (listener) listener.enabled = false;

                //  SceneManager.LoadScene("Opciones");


                //ahora cargamos de manera asincrona para que la musica no se pare (o de menos sensacion de que lo hace)
                SceneManager.LoadSceneAsync("Opciones");
            }
        }
           
    }

    private void FixedUpdate()
    {
        //para la velocidad del movimiento del personaje
        rigidbody2D.linearVelocity = entradaMovimiento * velocidad;
    }

    // --- Método para chequear encuentros en hierba ---
    private void ChequearHierba()
    {
        // Verificamos si estamos en hierba
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer))
        {
            // Acumulamos tiempo caminando
            cronometroPasos += Time.deltaTime;

            if (cronometroPasos >= tiempoEntreChequeos)
            {
                cronometroPasos = 0;

                // 1. Verificamos si tenemos Pokémon vivos para pelear
                var party = GetComponent<PokemonParty>();
                if (party.GetHealtyPokemon() == null) return; // Si todos están debilitados, no hay pelea

                if (Random.Range(0f, 100f) < probabilidad)
                {
                    // 2. Buscamos el Pokémon salvaje del área
                    var area = Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer).GetComponent<MapArea>();
                    if (area != null)
                    {
                        var wildPokemon = area.GetRandomWildPokemon();

                        // 3. Guardamos datos y cargamos escena
                        JugadorSpawn.posicion = transform.position;
                        JugadorSpawn.escenaAnterior = SceneManager.GetActiveScene().name;

                        // Necesitamos pasar estos datos al BattleSystem. 
                        // La forma más fácil con tu estructura actual es usar una clase estática temporal 
                        // o que el BattleSystem los busque al cargar.
                        BattleData.Party = party;
                        BattleData.WildPokemon = wildPokemon;

                        SceneManager.LoadScene("Combate");
                    }
                }
            }
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

    // Detectar si se presionó el botón de abrirMenu
    bool DetectarMenuOpciones()
    {
        if (esMovil && ControlesMoviles.Instance.botonMenuOpciones != null)
        {
            // Usar botón táctil en móvil
            return ControlesMoviles.Instance.botonMenuOpciones.SePresionoEsteFrame();
        }
        else
        {
            // Usar tecla espacio en PC
            return Input.GetKeyDown(KeyCode.X);
        }
    }


}
