using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    //Objeto que hace refencia al panel de opciones
    public GameObject panelOpciones;
    //boton volver del panel opciones
    public Button botonVolverPanelOpciones;
    //boton ir a controles
    public Button botonControles;

    //Objeto que hace refencia al panel de controles
    public GameObject panelControles;
    //boton volver del panel controles
    public Button botonVolverPanelControles;

    //desbloqueamos el cursor desde el principio del juego
    private void Awake()
    {
        //Para que el cursor se pueda mover
        Cursor.lockState = CursorLockMode.None;
        //Para que se vea
        Cursor.visible = true;


        //Son expresiones lambda que esperan a la activación del botón 

        botonVolverPanelOpciones.onClick.AddListener(() =>
        {
            volver();
        });

        botonControles.onClick.AddListener(() =>
        {
            panelOpciones.SetActive(false);
            panelControles.SetActive(true);
        });

        botonVolverPanelControles.onClick.AddListener(() =>
        {
            panelOpciones.SetActive(true);
            panelControles.SetActive(false);
        });
    }

    private void Update()
    {
        // --- Detectar X para cerrar menú ---
        if (Input.GetKeyDown(KeyCode.X))
        {
            volver();
        }
    }

    private void volver()
    {
        //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
        Cursor.lockState = CursorLockMode.Locked;
        //Y ahora le quitamos la visibilidad
        Cursor.visible = false;

        //Le asignamos la posición al jugador
        JugadorSpawn.posicion = GuardarPosicionAnterior.posicionAnterior;
        //Cambiamos de escena
        SceneManager.LoadScene(GuardarPosicionAnterior.escenaAnterior);

        //para que siempre se muestre primero la de opciones y no la de configuración
        panelOpciones.SetActive(true);
        panelControles.SetActive(false);
    }

}
