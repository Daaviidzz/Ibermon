using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactuable : MonoBehaviour
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

    //Parte de cuadro de dialogo
    //Con el TextArea le digo que me cree por defecto x cajas de x líneas como máximo
    //obviamente podrá añadir más
    [SerializeField, TextArea(1, 3)]
    private List<string> listaTextos;//creo una lista de textos
    [SerializeField]
    private ControladorTextosUI controladorTextosUI;//hace referencia al script que controlará la parte UI

    private int indice;//El contador de frases

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
            if (archivoAudio != null)
            {
                //Lanza el audio
                archivoAudio.PlayOneShot(audio);
            }

            //Si el animación no es nulo
            if (animacion != null)
            {
                // Lanzar animación una sola vez
                animacion.SetTrigger(triggerAnimacion);
            }

            if(listaTextos != null && listaTextos.Count > 0)
            {
                controladorTextosUI.activarDesactivarCajaDeTextos(true);//activamos la UI
                activarCartel();
            }

        }
    }

    private void activarCartel()
    {
        //si la posición es menor a el total de la lista
        if (indice < listaTextos.Count)
        {
            controladorTextosUI.mostrarTextos(listaTextos[indice]);//llamamos al método de la UI para que muestre el texto
            indice++;//sumamos uno al contador
        }
        else
        {
            controladorTextosUI.activarDesactivarCajaDeTextos(false);//basicamente cuando ya no queden más desactivamos la UI
            indice = 0;//cuando haya terminado ponemos que el indice sea 0
        }
    }

}
