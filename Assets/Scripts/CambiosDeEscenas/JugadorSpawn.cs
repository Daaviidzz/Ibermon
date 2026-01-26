using UnityEngine;
using UnityEngine.SceneManagement;

public class JugadorSpawn : MonoBehaviour
{
    //Variable para posicionar más adelante a nuestro personaje
    //pongo la variable estatica para poder acceder luego desde el script CambioDeEscena
    public static Vector2 posicion = Vector2.zero;

    private void Awake()
    {
        //para que el personaje no se destruya entre escenas
        //que tenga persistencia basicamente
        DontDestroyOnLoad(gameObject);
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
    {
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
