using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movimiento : MonoBehaviour
{
    //Variable para la velocidad del personaje
    private float velocidad;
    private float velocidadMinima = 3f;
    private float velocidadMaxima = 6f;

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

    }

    private void FixedUpdate()
    {
        //para la velocidad del movimiento del personaje
        rigidbody2D.linearVelocity = entradaMovimiento * velocidad;
    }

}