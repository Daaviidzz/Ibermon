using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Cambiado para usar TextMeshPro

public class ControllPortadaInicio : MonoBehaviour
{
    //Variable que almacena el número de la escena de destino
    //también podría ser un String y poner el nombre de la escena
    public string escenaDestino;

    //Texto de pulsar enter
    public TextMeshProUGUI textoPulsarEnter;
    //Velocidad de parpadeo del texto
    public float velocidadParpadeo = 2f;

    //bloqueamos el cursor desde el principio del juego
    private void Awake()
    {
        //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
        Cursor.lockState = CursorLockMode.Locked;
        //Y ahora le quitamos la visibilidad
        Cursor.visible = false;
    }

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
            SceneManager.LoadScene(escenaDestino);
        }

    }
}
