using UnityEngine;
using UnityEngine.SceneManagement;

public class ReproductorSonidoInteraccion : MonoBehaviour
{

    //Parte de sonido
    public AudioSource archivoAudio;
    public AudioClip audio;
    //Mensaje para la interacción
    public string mensaje = "Pulsa E para interactuar";

    //variable para saber si el jugador está dentro del colider
    private bool jugadorDentro = false;

    // Parte de animación
    public Animator animacion;// Donde colocaremos la animación
    public string triggerAnimacion; //Nombre del trigger

    private void OnTriggerEnter2D(Collider2D collision)
    { 
        if (collision.CompareTag("Player"))
        {
            jugadorDentro = true;
            //aquí debería de mostrar el mensaje lanzando la acción necesaria
        }
    }
    private void OnTriggerExit2D(Collider2D collision) 
    { 
        if (collision.CompareTag("Player"))
        {
            jugadorDentro = false;
        }
    }


    //Metodo para detectar colisiones 2D
    private void Update()
    {
        //Si el tag del objeto que detecta que colisiona es Player y ese player pulsa E
        if (jugadorDentro && Input.GetKeyDown(KeyCode.E))
        {
            //Lanza el audio
             archivoAudio.PlayOneShot(audio);

            //Si el animación no es nulo
            if (animacion != null)
            {
                // Lanzar animación una sola vez
                animacion.SetTrigger(triggerAnimacion);
            }
        }
    }
}
