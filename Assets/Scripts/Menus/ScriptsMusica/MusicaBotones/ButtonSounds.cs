using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSounds : MonoBehaviour, IPointerClickHandler, ISelectHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UIAudioManager.Instance?.PlayClick();
    }

    // Solo suena con flechas/teclado
    public void OnSelect(BaseEventData eventData)
    {
        UIAudioManager.Instance?.PlayHover();
    }
}