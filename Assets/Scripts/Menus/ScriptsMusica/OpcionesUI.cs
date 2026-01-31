using UnityEngine;
using UnityEngine.UI;

public class OpcionesUI : MonoBehaviour
{
    [SerializeField] private Slider sliderMusica;

    void Start()
    {
        if (sliderMusica == null)
        {
            Debug.LogError("SliderMusica NO asignado");
            return;
        }

        if (ControlMusica.instance == null)
        {
            Debug.LogError("ControlMusica NO existe (no se creó en PortadaInicio)");
            return;
        }

        sliderMusica.onValueChanged.RemoveAllListeners();
        sliderMusica.onValueChanged.AddListener(
            ControlMusica.instance.ControlMusicaVolumen
        );
    }
}
