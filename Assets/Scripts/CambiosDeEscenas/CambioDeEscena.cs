using UnityEngine;
using UnityEngine.SceneManagement;

public class CambioDeEscena : MonoBehaviour
{
    //Variable que almacena el número de la escena de destino
    //también podría ser un String y poner el nombre de la escena
    public int escenaDestino;
    //Variable que guarda donde aparecerá el personaje
    public Vector2 posicionSpawn;

    //Metodo para detectar colisiones 2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Si el tag del objeto que detecta que colisiona es Player
        if (collision.CompareTag("Player"))
        {
            //Le asignamos la posición al jugador
            JugadorSpawn.posicion = posicionSpawn;
            //Cambiamos de escena
            SceneManager.LoadScene(escenaDestino);
        }
    }
}
