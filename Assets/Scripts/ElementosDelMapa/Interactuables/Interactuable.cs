using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private PokemonParty equipoPokemon; // Referencia al componente que cura los Pokémon

    // NUEVO: Referencia al Rigidbody para frenar la inercia sin desactivar colisiones
    private Rigidbody2D rbPersonaje;


    private int indice;//El contador de frases
    private bool dialogoActivo = false; // Controla si estamos dentro de un diálogo


    //Parte movil
    [Header("Controles Móviles (Opcional)")]
    [SerializeField] private ActivacionBoton botonInteraccion; // Referencia al botón de interacción

    // Detectar si estamos en móvil o PC
    private bool esMovil;

    private void Awake()
    {
        comprobacionInicialParteMovil();

        if (GameObject.FindWithTag("Player") != null)
        {
            //Buscamos el componente de Movimiento del personaje para poder pararle
            movimientoPersonaje = GameObject.FindWithTag("Player").GetComponent<Movimiento>();
            // Buscamos el componente PokemonParty en el jugador para poder curarlo luego
            equipoPokemon = GameObject.FindWithTag("Player").GetComponent<PokemonParty>();

            // NUEVO: Pillamos el Rigidbody del jugador
            rbPersonaje = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        }
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
        //Si el tag del objeto que detecta que colisiona es Player y ese player pulsa E o el botón de interacción en caso móvil
        if (jugadorDentro && DetectarInteraccion())
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

            // --- Lógica especial para la Abuela ---
            // Si el objeto actual tiene el tag "Abuela", curamos a los pokemons
            if (gameObject.CompareTag("Abuela"))
            {
                if (equipoPokemon != null)
                {
                    equipoPokemon.HealAllPokemonsInParty();
                }
            }

            //Bloqueamos el movimiento del personaje y activamos el diálogo
            if (movimientoPersonaje != null) movimientoPersonaje.estaEnInteraccion = true;

            //FRENADO EN SECO SIN ATRAVESAR PAREDES
            if (rbPersonaje != null)
            {
                rbPersonaje.linearVelocity = Vector2.zero; // Anulamos la velocidad que llevaba
                // Congelamos la posición en X e Y para que no se deslice, pero sigue siendo Dynamic para que las paredes lo paren
                rbPersonaje.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            }

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
                // Si no hay textos, cerramos usando el nuevo método de limpieza
                TerminarDialogo();
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
                // Ya se acabaron las frases, cerramos todo con el método de limpieza
                TerminarDialogo();
            }
        }
        else
        {
            // No hay textos, cerramos inmediatamente
            TerminarDialogo();
        }
    }

    // NUEVO: Método para limpiar estados y devolver el control al jugador correctamente
    private void TerminarDialogo()
    {
        controladorTextosUI.activarDesactivarCajaDeTextos(false);
        indice = 0;
        dialogoActivo = false;

        // Desbloqueamos el movimiento del personaje
        if (movimientoPersonaje != null) movimientoPersonaje.estaEnInteraccion = false;

        // NUEVO: Quitamos el "freno de mano" (FreezePosition) pero mantenemos la rotación congelada para que no se caiga
        if (rbPersonaje != null)
        {
            rbPersonaje.constraints = RigidbodyConstraints2D.FreezeRotation;
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


    //Parte para movil


    //Parte movil inicial
    private void comprobacionInicialParteMovil()
    {
        // Detectar la plataforma
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif

        // Obtener referencias del script estatico de ControlesMoviles que estará asignado en UIMovil
        if (esMovil && ControlesMoviles.Instance != null)
        {
            botonInteraccion = ControlesMoviles.Instance.botonInteraccion;
        }

        // Desactivar controles en PC si no estamos en el editor probando
        if (!esMovil && ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.gameObject.SetActive(false);
        }
    }

    // Detectar si se presionó el botón de interaccion
    bool DetectarInteraccion()
    {
        if (esMovil && botonInteraccion != null)
        {
            // Usar botón táctil en móvil
            return botonInteraccion.SePresionoEsteFrame();
        }
        else
        {
            // Usar tecla E en PC
            return Input.GetKeyDown(KeyCode.E);
        }
    }

}