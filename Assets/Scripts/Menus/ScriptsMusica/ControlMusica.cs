using UnityEngine;
using UnityEngine.Audio;

public class ControlMusica : MonoBehaviour
{
    // Esto permite que el script sea accesible desde cualquier escena
    public static ControlMusica instance;

    [SerializeField] private AudioMixer audioMixer;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Para que no se pare al cambiar de escena
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // Evita que se duplique la música si vuelves al menú
        }
    }

    // Controla el volumen de la música según el valor del slider
    public void ControlMusicaVolumen(float sliderMusica)
    {
        float volume = sliderMusica > 0 ? Mathf.Log10(sliderMusica) * 20 : -80f;
        audioMixer.SetFloat("VolumenMusica", volume);
    }

    // Este método se llama cuando se inicia el juego desde el menú
    public void PararMusicaMenu()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        Destroy(gameObject); // Borramos el objeto del menú porque ya empezó el juego
    }
}