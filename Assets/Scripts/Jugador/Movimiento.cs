using Unity.Mathematics;
using UnityEngine;

public class Movimiento : MonoBehaviour
{
    //Variable para la velocidad del personaje
    public float velocidad = 1;

    //Variable para hacer referencia al RigidBody
    private Rigidbody2D rigidbody2D;

    //Variable para la animación
    private Animator animacion; // Cambiado a Animator


    //Metodo que se ejecuta nada más iniciar
    private void Awake()
    {
        //coger el rigidbody del objeto y ponerselo a la variable que hemos creado
        rigidbody2D = GetComponent<Rigidbody2D>();
        //lo mismo pero como la animación no está en el padre y está en el hijo haré
        animacion = GetComponentInChildren<Animator>();
    }

    //Este método es el que se usa para hacer el movimiento del personaje
    private void FixedUpdate()
    {
        //Variables que hacen las llamadas a los ejes del personaje
        float horizontal = Input.GetAxis("Horizontal");//Horizontal   
        float vertical = Input.GetAxis("Vertical");//Vertical

        //Para mover el personaje en sus ejes a la velocidad adecuada
        rigidbody2D.linearVelocity = new Vector2(horizontal, vertical) * velocidad;

        //llamo a mi animación, muy importante ponerle el mismo nombre que le pusimos en la animación
        //Con Math.Abs lo que haces es que te devuelva un número absoluto
        //Y dento le ponemos el valor final de la velocidad osea su Magnitud
        animacion.SetFloat("Camina", Mathf.Abs(rigidbody2D.linearVelocity.magnitude));

        //Horientación del personaje

        // Dirección horizontal
        if (horizontal > 0)
        {
            animacion.SetFloat("Direccion", 1);   // derecha
        }
        else if (horizontal < 0)
        {
            animacion.SetFloat("Direccion", -1);  // izquierda
        }
        else
        {
            animacion.SetFloat("Direccion", 0);   // sin dirección horizontal
        }

        // Dirección vertical
        if (vertical > 0)
        {
            animacion.SetFloat("DireccionY", 1);   // arriba
        }
        else if (vertical < 0)
        {
            animacion.SetFloat("DireccionY", -1);  // abajo
        }
        else
        {
            animacion.SetFloat("DireccionY", 0);   // sin dirección vertical
        }

    }
}