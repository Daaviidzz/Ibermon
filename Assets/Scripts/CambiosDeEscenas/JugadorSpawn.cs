using UnityEngine;
using UnityEngine.SceneManagement;

public class JugadorSpawn : MonoBehaviour
{
    //Variable para posicionar más adelante a nuestro personaje
    //pongo la variable estatica para poder acceder luego desde el script CambioDeEscena
    public static Vector2 posicion = Vector2.zero;

    //cambiado
    // Nueva variable para recordar en qué mapa estábamos antes de pelear
    public static string escenaAnterior = "";
    //cambiado
    private Movimiento scriptMovimiento;
    private Renderer renderizador;
    private Collider2D colisionador;

    private void Awake()
    {//cambiado
        //para que el personaje no se destruya entre escenas
        //que tenga persistencia basicamente
        DontDestroyOnLoad(gameObject);
        // Obtenemos referencias a los componentes que queremos apagar/encender
        scriptMovimiento = GetComponent<Movimiento>();
        renderizador = GetComponentInChildren<Renderer>(); // En hijo si el sprite está ahí
                                                           

        colisionador = GetComponent<Collider2D>();
    }

    //Estos 2 métodos no son estrictmente necesarios
    //pero son importantes para hacer las cosas mejor y más limpias

    //cuando este objeto empieza a estar activo, ejecuta esto
    private void OnEnable()
    {
        //con esto hacemos que cuando termines de cargar la escena avise 
        //y una vez este avisada ejecuta la función
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //este objeto deja de estar activo, ejecuta esto
    private void OnDisable()
    {
        //Deja de llamar a mi función cuando cargues una escena
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Esta función la llama Unity automáticamente justo DESPUÉS de cargar una escena nueva.
    // "scene" es la escena que se acaba de cargar.
    // "mode" indica cómo se ha cargado (normalmente Single).
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {//cambiado
        // CASO 1: Estamos en la escena de COMBATE
        if (scene.name == "Combate")
        {
            // Desactivamos el movimiento y la imagen del personaje de mapa
            if (scriptMovimiento) scriptMovimiento.enabled = false;
            if (renderizador) renderizador.enabled = false;
            if (colisionador) colisionador.enabled = false;
        }
        // CASO 2: Estamos en un MAPA normal (mundo, casa, etc)
        else
        {
            // Reactivamos todo
            if (scriptMovimiento) scriptMovimiento.enabled = true;
            if (renderizador) renderizador.enabled = true;
            if (colisionador) colisionador.enabled = true;
            // Comprobamos si tenemos guardada una posición de spawn.
            // Vector2.zero es (0,0). Lo usamos como "valor por defecto" para saber
            // si NO hemos guardado ninguna posición.
            if (posicion != Vector2.zero)
            {
                // Si sí tenemos una posición guardada, movemos al personaje a ese punto.
                // Básicamente: "aparece aquí cuando entres en esta escena".
                transform.position = posicion;
            }
        }
    }

}
