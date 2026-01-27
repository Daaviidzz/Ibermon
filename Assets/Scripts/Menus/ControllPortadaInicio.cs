using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllPortadaInicio : MonoBehaviour
{
    //Voy a cargar en esta primera escena el personaje y posicionarlo en la primera escena

    //Variable que almacena el número de la escena de destino
    //también podría ser un String y poner el nombre de la escena
    public int escenaDestino;
    //Variable que guarda donde aparecerá el personaje
    public Vector2 posicionSpawn;

    //El personaje a cargar
    public GameObject personaje;
    void Update()
    {
        //Para detectar si pulsa enter del teclado y el enter del teclado numerico
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
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
            SceneManager.LoadScene(1);
        }

    }
}
