using TMPro; // Cambiado para usar TextMeshPro
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ControllPortadaInicio : MonoBehaviour
{
    //Variable que almacena el numero de la escena de destino
    //tambien podria ser un String y poner el nombre de la escena
    public string escenaDestino;

    //Texto de pulsar enter
    public TextMeshProUGUI textoPulsarEnter;

    //Velocidad de parpadeo del texto
    public float velocidadParpadeo = 2f;

    // Detectar si estamos en movil o PC
    private bool esMovil;

    // Variable para evitar multiples cambios de escena
    private bool cambiandoEscena = false;

    //bloqueamos el cursor desde el principio del juego
    private void Awake()
    {
        comprobacionInicialParteMovil();

        // Cambiamos el texto dinamicamente segun la plataforma
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
            CursorSeguro.Bloquear();
        }
    }

    void Update()
    {
        //Hacemos parpadear el texto de pulsar enter
        if (textoPulsarEnter != null)
        {
            //Calculamos la transparencia del texto usando una funcion PingPong para que varie entre 0 y 1
            float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);
            //Asignamos la transparencia al color del texto
            Color c = textoPulsarEnter.color;
            //Asignamos el valor de alpha al canal alfa del color
            c.a = alpha;
            //Asignamos el color modificado al texto
            textoPulsarEnter.color = c;
        }

        // DETECCION DE ENTRADA - solo si no estamos ya cambiando de escena
        if (!cambiandoEscena && DetectarEntrada())
        {
            StartCoroutine(CambiarEscenaConBloqueo());
        }
    }

    // Corrutina para cambiar de escena bloqueando completamente los controles
    // Esto evita que el click de la portada active botones en la siguiente escena
    private IEnumerator CambiarEscenaConBloqueo()
    {
        cambiandoEscena = true;

        Debug.Log("[Portada] Iniciando cambio de escena");

        // Bloquear inmediatamente todos los controles moviles
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.DeshabilitarBotones();
            ControlesMoviles.Instance.LimpiarEstadoBotones();
        }

        // Esperar 2 frames para que Unity procese el bloqueo del raycast
        yield return null;
        yield return null;

        // Limpiar de nuevo por si acaso
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.LimpiarEstadoBotones();
        }

        // Pequeno delay adicional para asegurar que todo este limpio
        yield return new WaitForSeconds(0.15f);

        // Limpieza final antes de cambiar de escena
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.LimpiarEstadoBotones();
        }

        Debug.Log("[Portada] Cargando escena: " + escenaDestino);

        // Cambiar de escena
        SceneManager.LoadScene(escenaDestino);
    }

    // Metodo que unifica la deteccion segun la plataforma
    private bool DetectarEntrada()
    {
        if (esMovil)
        {
            // Detecta si hay al menos un toque en la pantalla
            bool tocoPantalla = Input.GetMouseButtonDown(0);
            //return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;//es este el que hay que poner pero para hacer pruebas desde el pc necesito el de abajo

            if (tocoPantalla)
            {
                Debug.Log("[Portada] Toque detectado en pantalla");

                // IMPORTANTE: Consumir todos los eventos de botones INMEDIATAMENTE
                // Esto previene que el click se propague a la siguiente escena
                if (ControlesMoviles.Instance != null)
                {
                    if (ControlesMoviles.Instance.botonInteraccion != null)
                        ControlesMoviles.Instance.botonInteraccion.SePresionoEsteFrame();
                    if (ControlesMoviles.Instance.botonCorrer != null)
                        ControlesMoviles.Instance.botonCorrer.SePresionoEsteFrame();
                    if (ControlesMoviles.Instance.botonMenuOpciones != null)
                        ControlesMoviles.Instance.botonMenuOpciones.SePresionoEsteFrame();
                }
            }

            return tocoPantalla;
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
        // Detectar la plataforma
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif
    }
}
