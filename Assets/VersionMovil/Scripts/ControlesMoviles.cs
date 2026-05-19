using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ControlesMoviles : MonoBehaviour
{
    public static ControlesMoviles Instance;

    [Header("Referencias UI")]
    public JoystickVirtual joystick;
    public ActivacionBoton botonInteraccion;
    public ActivacionBoton botonCorrer;
    public ActivacionBoton botonMenuOpciones;

    // CanvasGroup del objeto raiz para poder bloquear TODO de golpe
    private CanvasGroup canvasGroupPrincipal;

    private static readonly string[] escenasSinControles =
    {
        "PortadaInicio",
        "InicioCuenta",
        "MenuPrincipal",
        "Opciones",
        "Creditos",
        "Partidas"
    };

    private void Awake()
    {
        // Configuracion de la Instancia (Singleton)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            // Obtener o crear el CanvasGroup principal
            canvasGroupPrincipal = GetComponent<CanvasGroup>();
            if (canvasGroupPrincipal == null)
            {
                canvasGroupPrincipal = gameObject.AddComponent<CanvasGroup>();
            }

            SceneManager.sceneLoaded += AlCargarEscena;
            AplicarVisibilidadPorEscena(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ESTO ES LO QUE QUITA LOS CONTROLES EN PC
        // Solo se quedan activos si estamos en un movil REAL
#if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
#endif
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
        }
    }

    private void AlCargarEscena(Scene scene, LoadSceneMode mode)
    {
        LimpiarEstadoBotones();
        AplicarVisibilidadPorEscena(scene.name);
    }

    private void AplicarVisibilidadPorEscena(string nombreEscena)
    {
#if UNITY_ANDROID || UNITY_IOS
        bool mostrar = !EsEscenaSinControles(nombreEscena);

        if (canvasGroupPrincipal != null)
        {
            canvasGroupPrincipal.alpha = mostrar ? 1f : 0f;
            canvasGroupPrincipal.interactable = mostrar;
            canvasGroupPrincipal.blocksRaycasts = mostrar;
        }
#endif
    }

    private bool EsEscenaSinControles(string nombreEscena)
    {
        for (int i = 0; i < escenasSinControles.Length; i++)
        {
            if (escenasSinControles[i] == nombreEscena)
            {
                return true;
            }
        }

        return false;
    }

    // Metodo para mostrar solo el boton de interaccion durante los dialogos
    public void MostrarSoloInteraccion()
    {
        if (joystick != null) joystick.gameObject.SetActive(false);
        if (botonCorrer != null) botonCorrer.gameObject.SetActive(false);
        if (botonMenuOpciones != null) botonMenuOpciones.gameObject.SetActive(false);
        if (botonInteraccion != null) botonInteraccion.gameObject.SetActive(true);
    }

    // Metodo para mostrar todos los controles cuando volvemos al juego normal
    public void MostrarTodosLosControles()
    {
        if (joystick != null) joystick.gameObject.SetActive(true);
        if (botonCorrer != null) botonCorrer.gameObject.SetActive(true);
        if (botonMenuOpciones != null) botonMenuOpciones.gameObject.SetActive(true);
        if (botonInteraccion != null) botonInteraccion.gameObject.SetActive(true);
    }

    // Metodo para resetear el estado de todos los botones
    // Muy util cuando cambiamos de escena para evitar clicks fantasma
    public void LimpiarEstadoBotones()
    {
        if (botonInteraccion != null) botonInteraccion.ResetearEstado();
        if (botonCorrer != null) botonCorrer.ResetearEstado();
        if (botonMenuOpciones != null) botonMenuOpciones.ResetearEstado();

        Debug.Log("[ControlesMoviles] Estados de botones limpiados");
    }

    // Bloquear TODOS los controles temporalmente
    // Este metodo es mas agresivo que simplemente resetear el estado
    public void BloquearControlesTemporalmente(float duracion = 0.5f)
    {
        #if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(true);
        StartCoroutine(BloqueoTemporal(duracion));
        #endif
        // En PC no hacemos nada, los controles moviles estan desactivados
    }

    private IEnumerator BloqueoTemporal(float duracion)
    {
        Debug.Log($"[ControlesMoviles] BLOQUEANDO controles por {duracion}s");

        // Deshabilitar todos los botones individualmente
        if (botonInteraccion != null) botonInteraccion.Deshabilitar();
        if (botonCorrer != null) botonCorrer.Deshabilitar();
        if (botonMenuOpciones != null) botonMenuOpciones.Deshabilitar();

        // Bloquear tambien el raycast del canvas principal por si acaso
        if (canvasGroupPrincipal != null)
        {
            canvasGroupPrincipal.blocksRaycasts = false;
        }

        // Esperar el tiempo especificado
        yield return new WaitForSeconds(duracion);

        // Re-habilitar todo
        if (botonInteraccion != null) botonInteraccion.Habilitar();
        if (botonCorrer != null) botonCorrer.Habilitar();
        if (botonMenuOpciones != null) botonMenuOpciones.Habilitar();

        if (canvasGroupPrincipal != null)
        {
            canvasGroupPrincipal.blocksRaycasts = true;
        }

        Debug.Log("[ControlesMoviles] DESBLOQUEANDO controles");
    }

    // Deshabilitar todos los botones permanentemente hasta que se vuelvan a habilitar
    public void DeshabilitarBotones()
    {
        if (botonInteraccion != null) botonInteraccion.Deshabilitar();
        if (botonCorrer != null) botonCorrer.Deshabilitar();
        if (botonMenuOpciones != null) botonMenuOpciones.Deshabilitar();

        Debug.Log("[ControlesMoviles] Botones deshabilitados");
    }

    // Habilitar todos los botones de nuevo
    public void HabilitarBotones()
    {
        if (botonInteraccion != null) botonInteraccion.Habilitar();
        if (botonCorrer != null) botonCorrer.Habilitar();
        if (botonMenuOpciones != null) botonMenuOpciones.Habilitar();

        Debug.Log("[ControlesMoviles] Botones habilitados");
    }
}
