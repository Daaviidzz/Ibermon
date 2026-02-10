using UnityEngine;

public class ControlesMoviles : MonoBehaviour
{
    public static ControlesMoviles Instance;

    [Header("Referencias UI")]
    public JoystickVirtual joystick;
    public ActivacionBoton botonInteraccion;
    public ActivacionBoton botonCorrer;
    public ActivacionBoton botonMenuOpciones;

    private void Awake()
    {
        // Configuración de la Instancia (Singleton)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void MostrarSoloInteraccion()
    {
        if (joystick != null) joystick.gameObject.SetActive(false);
        if (botonCorrer != null) botonCorrer.gameObject.SetActive(false);
        if (botonMenuOpciones != null) botonMenuOpciones.gameObject.SetActive(false);

        if (botonInteraccion != null) botonInteraccion.gameObject.SetActive(true);
    }

    public void MostrarTodosLosControles()
    {
        if (joystick != null) joystick.gameObject.SetActive(true);
        if (botonCorrer != null) botonCorrer.gameObject.SetActive(true);
        if (botonMenuOpciones != null) botonMenuOpciones.gameObject.SetActive(true);

        if (botonInteraccion != null) botonInteraccion.gameObject.SetActive(true);
    }


}