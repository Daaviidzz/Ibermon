using UnityEngine;
using UnityEngine.SceneManagement;

public class ReproductorSonidoInteraccion : MonoBehaviour
{

    //Parte de sonido
    public AudioSource archivoAudio;
    public AudioClip audio;
    //Mensaje para la interacción
    public string mensaje = "Pulsa E para interactuar";


    private bool jugadorDentro = false;
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
                archivoAudio.PlayOneShot(audio);
        }
    }
}
