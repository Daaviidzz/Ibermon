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
        if (!musicaMenuParada &&
            scene.name != "Opciones" &&
            scene.name != "MenuPrincipal" &&
            scene.name != "PortadaInicio"&&
            scene.name != "Creditos" &&
            scene.name != "Partidas"
            )
        {
            PararMusicaMenu();
            musicaMenuParada = true;
        }
    }

    public void ControlMasterVolumen(float sliderValue)
    {
        float volume = sliderValue <= 0f ? -80f : Mathf.Log10(sliderValue) * 20f;
        audioMixer.SetFloat("ControlMaster", volume);
    }

    public void ControlMusicaVolumen(float sliderValue)
    {
        float volume = sliderValue <= 0f ? -80f : Mathf.Log10(sliderValue) * 20f;
        audioMixer.SetFloat("ControlMusica", volume);
    }

    public void ControlVocesVolumen(float sliderValue)
    {
        float volume = sliderValue <= 0f ? -80f : Mathf.Log10(sliderValue) * 20f;
        audioMixer.SetFloat("ControlVoces", volume);
    }

    public void PararMusicaMenu()
    {
        if (audioSource != null) audioSource.Stop();
    }
}