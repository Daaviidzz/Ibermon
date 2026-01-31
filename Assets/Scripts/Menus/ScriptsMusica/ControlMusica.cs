using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class ControlMusica : MonoBehaviour
{
    public static ControlMusica instance;

    [Header("Configuracion de Audio")]
    [SerializeField] public AudioMixer audioMixer;

    private AudioSource audioSource;
    private bool musicaMenuParada = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();

            // Eliminar el AudioListener de este objeto si lo tiene
            // No lo necesita, los AudioListener ya están en las cámaras
            var listener = GetComponent<AudioListener>();
            if (listener != null) Destroy(listener);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Solo parar la música del menú cuando entras al juego (una sola vez)
        if (!musicaMenuParada &&
            scene.name != "Opciones" &&
            scene.name != "MenuPrincipal" &&
            scene.name != "PortadaInicio")
        {
            PararMusicaMenu();
            musicaMenuParada = true;
        }
    }

    public void ControlMusicaVolumen(float sliderValue)
    {
        float volume = sliderValue <= 0f ? -80f : Mathf.Log10(sliderValue) * 20f;
        audioMixer.SetFloat("ControlMusica", volume);
    }

    public void PararMusicaMenu()
    {
        if (audioSource != null) audioSource.Stop();
    }
}