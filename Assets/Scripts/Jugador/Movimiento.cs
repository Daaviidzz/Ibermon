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
    public float probabilidad = 10f;    // 10% de probabilidad
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

    //Metodo que se ejecuta nada más iniciar
    private void Awake()
    {
        //coger el rigidbody del objeto y ponerselo a la variable que hemos creado
        rigidbody2D = GetComponent<Rigidbody2D>();
        //lo mismo pero como la animación no está en el padre y está en el hijo haré
        animacion = GetComponentInChildren<Animator>();

        //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
        Cursor.lockState = CursorLockMode.Locked;
        //Y ahora le quitamos la visibilidad
        Cursor.visible = false;

    }

    //Este método es el que se usa para hacer el movimiento del personaje
    private void Update()
    {
        if (!estaEnInteraccion)
        {
            //si pulsa shift irá más rapido y sino irá a la velocidad normal
            if (Input.GetKey(KeyCode.LeftShift))
            {
                velocidad = velocidadMaxima;
            }
            else
            {
                velocidad = velocidadMinima;
            }

            //colocamos dentro de nuestro vector2 el x con horizontal y el y con vertical
            entradaMovimiento.x = Input.GetAxis("Horizontal");//Horizontal   
            entradaMovimiento.y = Input.GetAxis("Vertical");//Vertical

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

            // --- Detectar X para abrir menú ---
            if (Input.GetKeyDown(KeyCode.X))
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

}