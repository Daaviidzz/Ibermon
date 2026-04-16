using UnityEngine;
using System.Collections;

public class ControlesMoviles : MonoBehaviour
{
    public static ControlesMoviles Instance;

    [Header("Referencias UI")]
    public JoystickVirtual joystick;
    public ActivacionBoton botonInteraccion;
    public ActivacionBoton botonCorrer;
    public ActivacionBoton botonMenuOpciones;

    // CanvasGroup del objeto raíz para poder bloquear TODO de golpe
    private CanvasGroup canvasGroupPrincipal;

    private void Awake()
    {
        // Configuración de la Instancia (Singleton)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Obtener o crear el CanvasGroup principal
            canvasGroupPrincipal = GetComponent<CanvasGroup>();
            if (canvasGroupPrincipal == null)
            {
                canvasGroupPrincipal = gameObject.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ESTO ES LO QUE QUITA LOS CONTROLES EN PC
        // Solo se quedan activos si estamos en un móvil REAL
#if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
#endif
    }

    // Método para mostrar solo el botón de interacción durante los diálogos
    public void MostrarSoloInteraccion()
    {
        if (joystick != null) joystick.gameObject.SetActive(false);
        if (botonCorrer != null) botonCorrer.gameObject.SetActive(false);
        if (botonMenuOpciones != null) botonMenuOpciones.gameObject.SetActive(false);
        if (botonInteraccion != null) botonInteraccion.gameObject.SetActive(true);
    }

    // Método para mostrar todos los controles cuando volvemos al juego normal
    public void MostrarTodosLosControles()
    {
        if (joystick != null) joystick.gameObject.SetActive(true);
        if (botonCorrer != null) botonCorrer.gameObject.SetActive(true);
        if (botonMenuOpciones != null) botonMenuOpciones.gameObject.SetActive(true);
        if (botonInteraccion != null) botonInteraccion.gameObject.SetActive(true);
    }

    // Método para resetear el estado de todos los botones
    // Muy útil cuando cambiamos de escena para evitar clicks fantasma
    public void LimpiarEstadoBotones()
    {
        if (botonInteraccion != null) botonInteraccion.ResetearEstado();
        if (botonCorrer != null) botonCorrer.ResetearEstado();
        if (botonMenuOpciones != null) botonMenuOpciones.ResetearEstado();

        Debug.Log("[ControlesMoviles] Estados de botones limpiados");
    }

    // Bloquear TODOS los controles temporalmente
    // Este método es más agresivo que simplemente resetear el estado
    public void BloquearControlesTemporalmente(float duracion = 0.5f)
    {
        gameObject.SetActive(true);
        StartCoroutine(BloqueoTemporal(duracion));
    }

    private IEnumerator BloqueoTemporal(float duracion)
    {
        Debug.Log($"[ControlesMoviles] BLOQUEANDO controles por {duracion}s");

        // Deshabilitar todos los botones individualmente
        if (botonInteraccion != null) botonInteraccion.Deshabilitar();
        if (botonCorrer != null) botonCorrer.Deshabilitar();
        if (botonMenuOpciones != null) botonMenuOpciones.Deshabilitar();

        // Bloquear también el raycast del canvas principal por si acaso
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