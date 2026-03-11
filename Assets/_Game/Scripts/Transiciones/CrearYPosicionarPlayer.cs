using UnityEngine;
using UnityEngine.SceneManagement;

public class CrearYPosicionarPlayer : MonoBehaviour
{
    //Variable que almacena el número de la escena de destino
    //también podría ser un String y poner el nombre de la escena
    public string escenaDestino;
    //Variable que guarda donde aparecerá el personaje
    public Vector2 posicionSpawn;

    //El personaje a cargar
    public GameObject personaje;

    public void crearEInstanciarPersonaje()
    {
        // Instanciamos el personaje SOLO si no existe ya
        if (GameObject.FindWithTag("Player") == null)
        {
            Instantiate(personaje);
        }

        //Le asignamos la posición al jugador
        JugadorSpawn.posicion = posicionSpawn;

        //Como esta escena sería la 0 la del menú principal y queremos acceder a la del juego que sería la siguiente
        //la escena a la que queremos acceder será la 1 (la de la casa del personaje)
        SceneManager.LoadScene(escenaDestino);
    }
}
