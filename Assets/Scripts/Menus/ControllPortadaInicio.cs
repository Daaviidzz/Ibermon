using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Cambiado para usar TextMeshPro

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
    //Texto de pulsar enter
    public TextMeshProUGUI textoPulsarEnter;
    //Velocidad de parpadeo del texto
    public float velocidadParpadeo = 2f;
    void Update()
    {
        //Hacemos parpadear el texto de pulsar enter
        if (textoPulsarEnter != null)
        {
            //Calculamos la transparencia del texto usando una función PingPong para que varíe entre 0 y 1
            float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);
            //Asignamos la transparencia al color del texto
            Color c = textoPulsarEnter.color;
            //Asignamos el valor de alpha al canal alfa del color
            c.a = alpha;
            //Asignamos el color modificado al texto
            textoPulsarEnter.color = c;
        }
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
