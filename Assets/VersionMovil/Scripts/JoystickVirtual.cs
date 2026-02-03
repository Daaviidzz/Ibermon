using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
// Este script permite crear un joystick virtual funcional
public class JoystickVirtual : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // Referencia a la imagen de la palanca que se moverá
    [SerializeField] private RectTransform palancaJoystick;
    // Radio máximo que puede moverse la palanca (en píxeles)
    [SerializeField] private float radioMaximo = 50f;
    // Referencia al RectTransform del fondo del joystick
    private RectTransform joystickBackground;
    // Vector2 que almacena la dirección del input (-1 a 1 en X y Y)
    private Vector2 inputVector;
    void Start()
    {
        // Obtener el RectTransform del objeto actual (el fondo del joystick)
        joystickBackground = GetComponent<RectTransform>();
        // Si no se asignó la palanca manualmente, intentar encontrarla
        if (palancaJoystick == null)
        {
            // Buscar un hijo llamado "JoystickHandle"
            palancaJoystick = transform.Find("JoystickHandle").GetComponent<RectTransform>();
        }
    }
    // Se ejecuta cuando el usuario toca el joystick
    public void OnPointerDown(PointerEventData eventData)
    {
        // Manejar el toque como si estuviera arrastrando
        OnDrag(eventData);
    }
    // Se ejecuta mientras el usuario arrastra el dedo
    public void OnDrag(PointerEventData eventData)
    {
        // Obtener la posición del toque en el espacio del Canvas
        Vector2 posicion;
        // Convertir la posición de la pantalla a la posición local del joystick
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
        joystickBackground, // El fondo del joystick
        eventData.position, // Posición del toque en pantalla
        eventData.pressEventCamera, // Cámara del Canvas (null si es Overlay)
        out posicion)) // Variable donde se guardará el resultado
        {
            // Normalizar la posición respecto al tamaño del joystick
            // Dividimos entre el tamaño del fondo para obtener valores relativos
            posicion.x = (posicion.x / joystickBackground.sizeDelta.x);
            posicion.y = (posicion.y / joystickBackground.sizeDelta.y);
            // Convertir a un vector centrado en (0, 0)
            // Multiplicamos por 2 y restamos 1 para que vaya de -1 a 1
            inputVector = new Vector2(posicion.x * 2, posicion.y * 2);
            // Limitar la magnitud del vector a 1 (crear un área circular)
            // Esto evita que el joystick se mueva más allá de su radio
            inputVector = (inputVector.magnitude > 1f) ? inputVector.normalized : inputVector;
            // Mover la palanca visualmente
            // Multiplicamos por radioMaximo para obtener la posición en píxeles
            palancaJoystick.anchoredPosition = new Vector2(
            inputVector.x * radioMaximo,
            inputVector.y * radioMaximo
            );
        }
    }
    // Se ejecuta cuando el usuario levanta el dedo
    public void OnPointerUp(PointerEventData eventData)
    {
        // Resetear el input a cero
        inputVector = Vector2.zero;
        // Volver la palanca al centro
        palancaJoystick.anchoredPosition = Vector2.zero;
    }
    // Función pública para obtener el input horizontal (-1 a 1)
    public float Horizontal()
    {
        return inputVector.x;
    }
    // Función pública para obtener el input vertical (-1 a 1)
    public float Vertical()
    {
        return inputVector.y;
    }
    // Función para obtener el vector de dirección completo
    public Vector2 ObtenerDireccion()
    {
        return inputVector;
    }
}