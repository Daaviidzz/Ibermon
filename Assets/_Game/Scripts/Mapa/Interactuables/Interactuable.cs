using Assets.Scripts.Batalla;
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

    //Si es entrenador o no
    public bool esEntrenador = false;

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

    // NUEVO: Variable para activar el cambio automático de fase según Pokémon capturados
    [Header("Cambio automático de fase")]
    [Tooltip("Si está marcado, la fase cambiará automáticamente según los Pokémon capturados")]
    public bool cambiarFaseSegunPokemon = false;
    [Tooltip("Número de Pokémon necesarios para cambiar a la fase 1")]
    public int pokemonNecesariosParaFase1 = 5;

    [Header("Cambio de escena al terminar fase 1")]
    [Tooltip("Si está marcado, al terminar la fase 1 cambia automáticamente a otra escena")]
    public bool cambiarEscenaAlTerminarFase1 = false;
    [Tooltip("Nombre de la escena a la que ir (ej: SalidaRuta1)")]
    public string escenaDespuesDeFase1 = "SalidaRuta1";
    [Tooltip("Segundos que durará la escena antes de volver")]
    public float segundosEnEscenaTransicion = 5f;

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

            // CRÍTICO: Limpiar el estado del botón de interacción al entrar al trigger
            // Esto evita que clicks antiguos fuera del área se activen cuando entramos
            if (esMovil && ControlesMoviles.Instance != null && ControlesMoviles.Instance.botonInteraccion != null)
            {
                ControlesMoviles.Instance.botonInteraccion.ResetearEstado();
                Debug.Log($"[{gameObject.name}] Jugador entro - Estado de interaccion limpiado");
            }
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
        // Si estamos en móvil y el botón aún no existe, lo reasignamos dinámicamente
        if (esMovil && botonInteraccion == null && ControlesMoviles.Instance != null)
        {
            botonInteraccion = ControlesMoviles.Instance.botonInteraccion;
        }

        // NUEVA VERIFICACIÓN: Solo procesar input si NO estamos ya en diálogo
        // Y si el jugador está dentro del collider
        if (jugadorDentro && !dialogoActivo && DetectarInteraccion())
        {
            // NUEVO: Antes de mostrar el diálogo, comprobar si hay que cambiar de fase automáticamente
            if (cambiarFaseSegunPokemon && equipoPokemon != null)
            {
                ActualizarFaseSegunPokemon();
            }

            // Si este objeto no tiene controladorTextosUI ni fasesDialogo, no hace nada con textos
            // Solo ejecuta audio y animación si existen
            if (controladorTextosUI == null || fasesDialogo == null || fasesDialogo.Count == 0)
            {
                if (archivoAudio != null) archivoAudio.PlayOneShot(audio);
                if (animacion != null) animacion.SetTrigger(triggerAnimacion);
                return; // Sale y no intenta abrir diálogo
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

            //ocultar controles móviles excepto interacción
            if (esMovil && ControlesMoviles.Instance != null)
            {
                ControlesMoviles.Instance.MostrarSoloInteraccion();
            }


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
        // Si YA estamos en diálogo y detectamos input, avanzamos
        else if (dialogoActivo && jugadorDentro && DetectarInteraccion())
        {
            activarCartel();
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

    //  Método para limpiar estados y devolver el control al jugador correctamente
    private void TerminarDialogo()
    {
        controladorTextosUI.activarDesactivarCajaDeTextos(false);
        indice = 0;
        dialogoActivo = false;

        // Desbloqueamos el movimiento del personaje
        if (movimientoPersonaje != null) movimientoPersonaje.estaEnInteraccion = false;

        //  Quitamos el "freno de mano" (FreezePosition) pero mantenemos la rotación congelada para que no se caiga
        if (rbPersonaje != null)
        {
            rbPersonaje.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        //  Si acabamos de terminar la fase 1 y está activado el cambio de escena, lo hacemos
        if (cambiarEscenaAlTerminarFase1 && faseActual == 1)
        {
            CambiarAEscenaTransicion();
        }

        // restaurar controles móviles
        if (esMovil && ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.MostrarTodosLosControles();
        }
        // NUEVO: si es entrenador, lanzar batalla al cerrar el diálogo
        if (esEntrenador)
        {
            var trainerParty = GetComponent<PokemonParty>(); // el NPC necesita este componente
            if (trainerParty != null)
            {
                BattleData.TrainerParty = trainerParty;
                BattleData.EsEntrenador = true;
                BattleData.WildPokemon = null;

                JugadorSpawn.escenaAnterior = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                JugadorSpawn.posicion = GameObject.FindWithTag("Player").transform.position;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Batalla"); // tu escena de batalla
            }
        }


    }

    //  Método simple que solo cambia a la escena de transición
    private void CambiarAEscenaTransicion()
    {
        // Guardar la escena actual y posición del jugador
        JugadorSpawn.escenaAnterior = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        JugadorSpawn.posicion = GameObject.FindWithTag("Player").transform.position;

        // Cambiar a la escena de transición (que tiene el script AutoVolverEscena)
        UnityEngine.SceneManagement.SceneManager.LoadScene(escenaDespuesDeFase1);
    }

    //  Método que actualiza la fase según cuántos Pokémon tiene el jugador
    private void ActualizarFaseSegunPokemon()
    {
        int cantidadPokemon = equipoPokemon.Pokemons.Count;

        // Si tiene 5 o más Pokémon, cambia a la fase 1
        if (cantidadPokemon >= pokemonNecesariosParaFase1)
        {
            CambiarFase(1);
        }
        // Si tiene menos, mantiene la fase 0
        else
        {
            CambiarFase(0);
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
        // CRÍTICO: Solo permitir interacción si el jugador ESTÁ DENTRO del collider
        // Esto evita que clicks fuera del área se cuelen cuando entramos después
        if (!jugadorDentro)
            return false;

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