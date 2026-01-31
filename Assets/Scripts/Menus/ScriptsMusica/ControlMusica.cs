using UnityEngine;
using UnityEngine.Audio; 

public class ControlMusica : MonoBehaviour
{
    public static ControlMusica instance;

    [Header("Configuracion de Audio")]
    [SerializeField] private AudioMixer audioMixer;
    private AudioSource audioSource;


    //Para que no se destruya al cambiar de escena
    void Awake()
    {
     
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ControlMusicaVolumen(float sliderValue)
    {
        
        float volume = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20 : -80f;
        audioMixer.SetFloat("ControlMusica", volume);
    }

    public void PararMusicaMenu()
    {
        if (audioSource != null) audioSource.Stop();
        Destroy(gameObject);
    }
}