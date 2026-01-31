using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicaEscena : MonoBehaviour
{
    private AudioSource audioSource;
    private string escenaOrigen;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        // Guardamos de qué escena viene esta música
        escenaOrigen = SceneManager.GetActiveScene().name;
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
        // En Opciones: sigue sonando
        if (scene.name == "Opciones") return;

        // En Combate: pausar
        if (scene.name == "Combate")
        {
            if (audioSource && audioSource.isPlaying) audioSource.Pause();
            return;
        }

        // Si volvemos a la escena donde vino esta música: despausa
        if (scene.name == escenaOrigen)
        {
            if (audioSource)
            {
                audioSource.Play();
            }
            return;
        }

        // Cualquier otra escena: ya no hace falta, se destruye
        Destroy(gameObject);
    }
}