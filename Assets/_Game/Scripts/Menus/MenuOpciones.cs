using ApiRest.Managers;
using ApiRest.Models;
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
    // Boton para guardar la partida
    public Button botonGuardar;
    // Texto de confirmacion que aparece al guardar correctamente
    public GameObject textoGuardadoOk;

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

        // Ocultamos el texto de confirmacion al arrancar
        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }

        botonVolverPanelOpciones.onClick.AddListener(Volver);

        botonControles.onClick.AddListener(AbrirControles);

        botonVolverPanelControles.onClick.AddListener(VolverAOpciones);

        botonSalirJuego.onClick.AddListener(SalirJuego);

        botonGuardar.onClick.AddListener(GuardarPartida);
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
        // Solo bloquear cursor en PC
        if (!esMovil)
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

    // Envia la posicion y escena actuales del jugador a la API
    private void GuardarPartida()
    {
        if (!SessionManager.Instance.TienePartida)
        {
            Debug.LogWarning("[MenuOpciones] No hay partida activa, no se puede guardar");
            return;
        }

        // Usamos GuardarPosicionAnterior que se asigna justo al abrir el menu
        // es la posicion exacta donde estaba el jugador antes de entrar a opciones
        string escena = GuardarPosicionAnterior.escenaAnterior;
        float x = GuardarPosicionAnterior.posicionAnterior.x;
        float y = GuardarPosicionAnterior.posicionAnterior.y;

        if (string.IsNullOrEmpty(escena))
        {
            Debug.LogWarning("[MenuOpciones] La escena anterior no esta registrada todavia");
            return;
        }

        string partidaId = SessionManager.Instance.PartidaId;

        ApiSetup.Partida.ActualizarPosicion(partidaId, escena, x, y,
            ManejarGuardadoExitoso, ManejarGuardadoFallido);
    }

    // Se ejecuta cuando la API confirma que se guardo correctamente
    // Muestra un mensaje de confirmacion al jugador
    private void ManejarGuardadoExitoso(PartidaCompleta partidaActualizada)
    {
        Debug.Log("[MenuOpciones] Partida guardada correctamente");

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(true);
        }
    }

    // Se ejecuta si la API devuelve error al guardar
    private void ManejarGuardadoFallido(string mensajeError)
    {
        Debug.LogError($"[MenuOpciones] Error al guardar partida: {mensajeError}");
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