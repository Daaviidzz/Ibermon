using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    // Objeto que hace referencia al panel de opciones
    public GameObject panelOpciones;
    // Boton volver del panel opciones
    public Button botonVolverPanelOpciones;
    // Boton ir a controles
    public Button botonControles;
    public Button botonSalirJuego;

    // Objeto que hace referencia al panel de controles
    public GameObject panelControles;
    // Boton volver del panel controles
    public Button botonVolverPanelControles;

    // Detectar si estamos en movil o PC
    private bool esMovil;

    // Desbloqueamos el cursor desde el principio del juego
    private void Awake()
    {
        comprobacionInicialParteMovil();

        // Solo desbloquear cursor en PC
        if (!esMovil)
        {
            // Para que el cursor se pueda mover
            Cursor.lockState = CursorLockMode.None;
            // Para que se vea
            Cursor.visible = true;
        }

        botonVolverPanelOpciones.onClick.AddListener(Volver);
        botonControles.onClick.AddListener(AbrirControles);
        botonVolverPanelControles.onClick.AddListener(VolverAOpciones);
        //botonSalirJuego.onClick.AddListener(SalirJuego); como ahora no se usa pues lo comento para que no explote
    }

    private void Update()
    {
        // Detectar X para cerrar menu
        if (Input.GetKeyDown(KeyCode.X))
        {
            Volver();
        }
    }

    // Abre el panel de controles y cierra el de opciones
    private void AbrirControles()
    {
        panelOpciones.SetActive(false);
        panelControles.SetActive(true);
    }

    // Vuelve al panel de opciones desde el de controles
    private void VolverAOpciones()
    {
        panelOpciones.SetActive(true);
        panelControles.SetActive(false);
    }

    // Cierra el menu y vuelve al mapa donde estaba el jugador
    private void Volver()
    {
        // Solo bloquear cursor en PC si el UIOpcionesPanel NO está abierto
        // Si está abierto, él mismo gestiona el cursor al volver
        if (!esMovil && !UIOpcionesPanel.estaAbierto)
        {
            // Para que el cursor se quede en el medio
            Cursor.lockState = CursorLockMode.Locked;
            // Y ahora le quitamos la visibilidad
            Cursor.visible = false;
        }

        // Limpiar el estado de todos los botones moviles antes de cambiar de escena
        // Esto previene que clicks pendientes activen cosas en la escena siguiente
        if (ControlesMoviles.Instance != null)
        {
            ControlesMoviles.Instance.LimpiarEstadoBotones();
        }

        // Le asignamos la posicion al jugador
        JugadorSpawn.posicion = GuardarPosicionAnterior.posicionAnterior;

        // Cambiamos de escena
        SceneManager.LoadScene(GuardarPosicionAnterior.escenaAnterior);

        // Para que siempre se muestre primero la de opciones y no la de configuracion
        panelOpciones.SetActive(true);
        panelControles.SetActive(false);
    }

    // Cierra el juego
    private void SalirJuego()
    {
        Application.Quit();
    }

    // Parte movil inicial
    private void comprobacionInicialParteMovil()
    {
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif
    }
}