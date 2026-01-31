using UnityEngine;
using UnityEngine.Audio;
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

        // Leer el volumen actual del AudioMixer y ponerlo en el slider
        float volumenActual;
        ControlMusica.instance.audioMixer.GetFloat("ControlMusica", out volumenActual);
        // Convertir de dB a valor linear (inverso de lo que hace ControlMusicaVolumen)
        sliderMusica.value = volumenActual <= -80f ? 0f : Mathf.Pow(10f, volumenActual / 20f);

        sliderMusica.onValueChanged.RemoveAllListeners();
        sliderMusica.onValueChanged.AddListener(
            ControlMusica.instance.ControlMusicaVolumen
        );
    }
}
