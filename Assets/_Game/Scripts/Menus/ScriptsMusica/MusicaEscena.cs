using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MusicaEscena : MonoBehaviour
{
    private AudioSource audioSource;
    private string escenaOrigen;

    // Diccionario para trackear qué escena tiene música activa
    private static Dictionary<string, MusicaEscena> musicaPorEscena = new Dictionary<string, MusicaEscena>();

    private void Awake()
    {
        escenaOrigen = SceneManager.GetActiveScene().name;

        // Si ya existe música para esta escena, destruir esta nueva
        if (musicaPorEscena.ContainsKey(escenaOrigen) && musicaPorEscena[escenaOrigen] != null)
        {
            Destroy(gameObject);
            return;
        }

        // Registrar esta música para su escena
        musicaPorEscena[escenaOrigen] = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
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
        // En Opciones: sigue sonando siempre
        if (scene.name == "Opciones")
        {
            return;
        }

        // Si estamos en nuestra escena de origen: reproducir
        if (scene.name == escenaOrigen)
        {
            if (audioSource && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            return;
        }

        // Cualquier otra escena: destruir y limpiar del diccionario
        if (musicaPorEscena.ContainsKey(escenaOrigen) && musicaPorEscena[escenaOrigen] == this)
        {
            musicaPorEscena.Remove(escenaOrigen);
        }
        Destroy(gameObject);
    }
}