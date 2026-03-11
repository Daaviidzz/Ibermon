using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OpcionesUI : MonoBehaviour
{
    [SerializeField] private Slider sliderMaster;
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderVoces;

    void Start()
    {
        if (sliderMaster == null || sliderMusica == null || sliderVoces == null)
        {
            Debug.LogError("Sliders NO asignados");
            return;
        }

        if (ControlMusica.instance == null)
        {
            Debug.LogError("ControlMusica NO existe");
            return;
        }

        // Leer volúmenes actuales del AudioMixer
        float volumenMaster, volumenMusica, volumenVoces;
        ControlMusica.instance.audioMixer.GetFloat("ControlMaster", out volumenMaster);
        ControlMusica.instance.audioMixer.GetFloat("ControlMusica", out volumenMusica);
        ControlMusica.instance.audioMixer.GetFloat("ControlVoces", out volumenVoces);

        // Convertir de dB a valor linear
        sliderMaster.value = volumenMaster <= -80f ? 0f : Mathf.Pow(10f, volumenMaster / 20f);
        sliderMusica.value = volumenMusica <= -80f ? 0f : Mathf.Pow(10f, volumenMusica / 20f);
        sliderVoces.value = volumenVoces <= -80f ? 0f : Mathf.Pow(10f, volumenVoces / 20f);

        // Conectar sliders
        sliderMaster.onValueChanged.RemoveAllListeners();
        sliderMaster.onValueChanged.AddListener(ControlMusica.instance.ControlMasterVolumen);

        sliderMusica.onValueChanged.RemoveAllListeners();
        sliderMusica.onValueChanged.AddListener(ControlMusica.instance.ControlMusicaVolumen);

        sliderVoces.onValueChanged.RemoveAllListeners();
        sliderVoces.onValueChanged.AddListener(ControlMusica.instance.ControlVocesVolumen);
    }
}