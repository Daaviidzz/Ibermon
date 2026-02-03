using UnityEngine;
using UnityEngine.EventSystems;

// Este script detecta cuando se presiona el botón
public class ActivacionBoton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Variable que indica si el botón está siendo presionado
    private bool botonPresionado = false;
    // Variable para detectar el momento exacto en que se presiona (solo un frame)
    private bool botonPresionadoEsteFrame = false;
    void Update()
    {
        // Resetear la variable de "presionado este frame" cada frame
        // Esto asegura que solo sea true durante un frame
        botonPresionadoEsteFrame = false;
    }
    // Se ejecuta cuando el usuario toca el botón
    public void OnPointerDown(PointerEventData eventData)
    {
        // Marcar que el botón está siendo presionado
        botonPresionado = true;
        // Marcar que se presionó en este frame específico
        botonPresionadoEsteFrame = true;
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
        return botonPresionado;
    }
    // Función similar a Input.GetButtonDown (presionar una vez)
    public bool SePresionoEsteFrame()
    {
        return botonPresionadoEsteFrame;
    }
}
