using TMPro; // Cambiado para usar TextMeshPro
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllPortadaInicio : MonoBehaviour
{
    //Variable que almacena el número de la escena de destino
    //también podría ser un String y poner el nombre de la escena
    public string escenaDestino;

    //Texto de pulsar enter
    public TextMeshProUGUI textoPulsarEnter;
    //Velocidad de parpadeo del texto
    public float velocidadParpadeo = 2f;

    // Detectar si estamos en móvil o PC
    private bool esMovil;

    //bloqueamos el cursor desde el principio del juego
    private void Awake()
    {
        comprobacionInicialParteMovil();

        // Cambiamos el texto dinámicamente según la plataforma
        if (textoPulsarEnter != null)
        {
            if (esMovil)
            {
                textoPulsarEnter.text = "TOCA LA PANTALLA";
            }
            else
            {
                textoPulsarEnter.text = "PULSA ENTER";
            }
        }

        // Solo bloquear cursor en PC
        if (!esMovil)
        {
            //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
            Cursor.lockState = CursorLockMode.Locked;
            //Y ahora le quitamos la visibilidad
            Cursor.visible = false;
        }
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

        // DETECCIÓN DE ENTRADA
        if (DetectarEntrada())
        {
            SceneManager.LoadScene(escenaDestino);
        }
    }

    // Método que unifica la detección según la plataforma
    private bool DetectarEntrada()
    {
        if (esMovil)
        {
            // Detecta si hay al menos un toque en la pantalla
            //return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;//es este el que hay que poner pero para hacer pruebas desde el pc necesito el de abajo
            return Input.GetMouseButtonDown(0);

        }
        else
        {
            // Para detectar si pulsa enter del teclado y el enter del teclado numerico
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        }
    }


    //Parte movil inicial
    private void comprobacionInicialParteMovil()
    {
        // Detectar la plataforma (Incluimos Editor para poder probarlo)
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        esMovil = true;
#else
        esMovil = false;
#endif
    }
}