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
        //si pulsa shift irá más rapido y sino irá a la velocidad normal
        if(Input.GetKey(KeyCode.LeftShift))
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
        animacion.SetFloat("Horizontal",entradaMovimiento.x);
        animacion.SetFloat("Vertical",entradaMovimiento.y);
        animacion.SetFloat("Velocidad",entradaMovimiento.magnitude);

        // --- Llamada a la función de chequeo de hierba ---
        // Solo chequeamos si el jugador se está moviendo realmente
        if (entradaMovimiento.magnitude > 0.1f)
        {
            ChequearHierba();
        }

    }

    private void FixedUpdate()
    {
        //para la velocidad del movimiento del personaje
        rigidbody2D.linearVelocity = entradaMovimiento * velocidad;
    }

    private void ChequearHierba()
    {
        // Detecta si hay algo de la capa "grassLayer" en la posición del jugador
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer))
        {
            cronometroPasos += Time.deltaTime;

            if (cronometroPasos >= tiempoEntreChequeos)
            {
                cronometroPasos = 0; // Reiniciamos el tiempo

                // Si el número aleatorio es menor que la probabilidad, entramos en combate
                if (Random.Range(0f, 100f) < probabilidad)
                {
                    

                    // 1. IMPORTANTE: Guardamos la posición actual EXACTA antes de irnos
                    JugadorSpawn.posicion = transform.position;

                    // 2. Guardamos el nombre de la escena actual para saber a cual volver luego
                    
                    JugadorSpawn.escenaAnterior = SceneManager.GetActiveScene().name;

                    // 3. Cargamos la batalla
                    SceneManager.LoadScene("Combate");
                }
            }
        }
    }

}