using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Este script detecta cuando se presiona el botón
public class ActivacionBoton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Variable que indica si el botón está siendo presionado
    private bool botonPresionado = false;

    // Variable para detectar el momento exacto en que se presiona (solo un frame)
    private bool botonPresionadoEsteFrame = false;

    // Sistema de cooldown para evitar clicks fantasma entre escenas
    private float tiempoUltimaActivacion = -999f;
    private const float COOLDOWN_MINIMO = 0.3f; // 300ms de seguridad

    // Referencia al CanvasGroup para poder bloquear el raycast
    private CanvasGroup canvasGroup;

    // Flag para saber si el botón está habilitado para recibir clicks
    private bool habilitado = true;

    private void Awake()
    {
        // Intentar obtener el CanvasGroup, si no existe lo creamos
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // Se ejecuta cuando el usuario toca el botón
    public void OnPointerDown(PointerEventData eventData)
    {
        // Si el botón está deshabilitado, ignorar completamente
        if (!habilitado)
        {
            Debug.Log($"[{gameObject.name}] CLICK BLOQUEADO - Boton deshabilitado");
            return;
        }

        // Solo aceptar el click si ha pasado suficiente tiempo desde el último
        // Esto evita que clicks de otras escenas se cuelen aquí
        if (Time.time - tiempoUltimaActivacion < COOLDOWN_MINIMO)
        {
            Debug.Log($"[{gameObject.name}] CLICK BLOQUEADO - Cooldown activo ({Time.time - tiempoUltimaActivacion:F2}s)");
            return;
        }

        // Marcar que el botón está siendo presionado
        botonPresionado = true;

        // Marcar que se presionó en este frame específico
        botonPresionadoEsteFrame = true;

        // Guardar el momento de esta activación
        tiempoUltimaActivacion = Time.time;

        Debug.Log($"[{gameObject.name}] CLICK ACEPTADO en tiempo {Time.time:F2}");
    }

    // Se ejecuta cuando el usuario levanta el dedo del botón
    public void OnPointerUp(PointerEventData eventData)
    {
        // Marcar que el botón ya no está presionado
        botonPresionado = false;
    }

    // Función similar a Input.GetButton (mantener presionado)
    public bool EstaPresionado()
    {
        return botonPresionado && habilitado;
    }

    // Función similar a Input.GetButtonDown (presionar una vez)
    public bool SePresionoEsteFrame()
    {
        // Solo devolver true si el botón está habilitado
        if (!habilitado)
            return false;

        // Consumimos el evento: si es true, lo devolvemos y lo ponemos a false inmediatamente
        if (botonPresionadoEsteFrame)
        {
            botonPresionadoEsteFrame = false;
            Debug.Log($"[{gameObject.name}] Evento consumido");
            return true;
        }
        return false;
    }

    // Método para resetear completamente el estado del botón
    // Útil cuando cambiamos de escena o queremos asegurarnos de que no hay eventos pendientes
    public void ResetearEstado()
    {
        botonPresionado = false;
        botonPresionadoEsteFrame = false;
        Debug.Log($"[{gameObject.name}] Estado reseteado");
    }

    // Deshabilitar el botón completamente bloqueando el raycast
    // Esto hace que Unity ni siquiera detecte los clicks sobre este botón
    public void Deshabilitar()
    {
        habilitado = false;
        botonPresionado = false;
        botonPresionadoEsteFrame = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        Debug.Log($"[{gameObject.name}] DESHABILITADO - Raycast bloqueado");
    }

    // Habilitar el botón de nuevo permitiendo que reciba clicks
    public void Habilitar()
    {
        habilitado = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        Debug.Log($"[{gameObject.name}] HABILITADO - Raycast activo");
    }

    // Cuando el botón se desactiva, limpiamos todo
    private void OnDisable()
    {
        ResetearEstado();
        habilitado = false;
    }

    // Cuando el botón se activa, también limpiamos por seguridad
    private void OnEnable()
    {
        botonPresionado = false;
        botonPresionadoEsteFrame = false;
        habilitado = true;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }
}