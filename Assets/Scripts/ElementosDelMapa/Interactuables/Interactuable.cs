using System.Collections.Generic;
using UnityEngine;

// Clase que representa una fase de diálogo del NPC
// Básicamente es una lista de frases que pertenecen a una fase concreta
[System.Serializable]
public class FaseDialogo
{
    [TextArea(1, 3)]
    public List<string> textosDeEstaFase; // Lista de frases de esta fase
}

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

    // Lista de fases que tendrá este NPC
    // Cada fase contiene su propia lista de textos
    public List<FaseDialogo> fasesDialogo;

    // Fase actual en la que se encuentra este NPC
    // Cada NPC puede tener más o menos fases, no pasa nada
    public int faseActual = 0;

    //Parte de cuadro de dialogo
    [SerializeField]
    private ControladorTextosUI controladorTextosUI;//hace referencia al script que controlará la parte UI

    //variable de movimiento de personaje que luego usaremos
    private Movimiento movimientoPersonaje;

    private int indice;//El contador de frases
    private bool dialogoActivo = false; // Controla si estamos dentro de un diálogo


    private void Awake()
    {
        //para coger el script de movimiento del personaje con tag Player
        movimientoPersonaje = GameObject.FindWithTag("Player").GetComponent<Movimiento>();
    }

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
            // Si este objeto no tiene controladorTextosUI ni fasesDialogo, no hace nada con textos
            // Solo ejecuta audio y animación si existen
            if (controladorTextosUI == null || fasesDialogo == null || fasesDialogo.Count == 0)
            {
                if (archivoAudio != null) archivoAudio.PlayOneShot(audio);
                if (animacion != null) animacion.SetTrigger(triggerAnimacion);
                return; // Sale y no intenta abrir diálogo
            }

            // Si ya estamos en diálogo, avanza la siguiente frase
            // El return evita que se ejecute lo de abajo en el mismo frame
            if (dialogoActivo)
            {
                activarCartel();
                return;
            }

            //Bloqueamos el movimiento del personaje y activamos el diálogo
            movimientoPersonaje.estaEnInteraccion = true;
            dialogoActivo = true;

            //Lanza el audio si existe
            if (archivoAudio != null) archivoAudio.PlayOneShot(audio);

            //Lanza animación si existe
            if (animacion != null) animacion.SetTrigger(triggerAnimacion);

            // Comprobamos si la fase actual tiene textos
            // Usamos && para que si la lista es nula no intente hacer .Count
            if (fasesDialogo[faseActual].textosDeEstaFase != null && fasesDialogo[faseActual].textosDeEstaFase.Count > 0)
            {
                //Activamos la UI y mostramos la primera frase de la fase actual
                controladorTextosUI.activarDesactivarCajaDeTextos(true);
                activarCartel();
            }
            else
            {
                //Si no hay textos, no tiene sentido abrir la UI
                //Simplemente desbloqueamos el movimiento y cerramos el diálogo
                dialogoActivo = false;
                movimientoPersonaje.estaEnInteraccion = false;
            }
        }
    }

    private void activarCartel()
    {
        // Cogemos la lista de frases de la fase actual del NPC
        List<string> textos = fasesDialogo[faseActual].textosDeEstaFase;

        // Si por lo que sea la lista está vacía o es nula, cerramos y desbloqueamos
        // Usamos && para que si la lista es nula no intente hacer .Count
        if (textos != null && textos.Count > 0)
        {
            //si la posición es menor a el total de la lista
            if (indice < textos.Count)
            {
                controladorTextosUI.mostrarTextos(textos[indice]);//llamamos al método de la UI para que muestre el texto
                indice++;//sumamos uno al contador
            }
            else
            {
                // Ya se acabaron las frases, cerramos todo
                controladorTextosUI.activarDesactivarCajaDeTextos(false);//basicamente cuando ya no queden más desactivamos la UI
                indice = 0;//cuando haya terminado ponemos que el indice sea 0
                dialogoActivo = false; // Cerramos el diálogo
                //Desbloqueamos el movimiento del personaje
                movimientoPersonaje.estaEnInteraccion = false;
            }
        }
        else
        {
            // No hay textos, cerramos inmediatamente
            controladorTextosUI.activarDesactivarCajaDeTextos(false);
            indice = 0;
            dialogoActivo = false;
            movimientoPersonaje.estaEnInteraccion = false;
        }
    }


    // Método para cambiar de fase del NPC
    // Lo puedes llamar desde cualquier script cuando el jugador haga algo
    public void CambiarFase(int nuevaFase)
    {
        //Comprobamos que la fase existe para evitar errores
        if (nuevaFase >= 0 && nuevaFase < fasesDialogo.Count)
        {
            faseActual = nuevaFase;
        }
    }

}