using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TorrenteFrases : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> frasesTorrente = new List<AudioClip>();
    public float tiempoEntreFrases = 300f;
    private int ultimoIndice = -1;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si entramos en combate, paramos las frases
        if (scene.name == "Combate")
        {
            DetenerFrases();
        }
        else
        {
            // SI NO es combate (es decir, hemos vuelto al mundo), las reactivamos
            ReanudarFrases();
        }
    }

    void Start()
    {
        // Iniciamos el ciclo normalmente
        ReanudarFrases();
    }

    void ReproducirFraseRandom()
    {
        if (frasesTorrente.Count == 0) return;

        int indice = Random.Range(0, frasesTorrente.Count);

        if (indice == ultimoIndice && frasesTorrente.Count > 1)
        {
            indice = (indice + 1) % frasesTorrente.Count;
        }

        ultimoIndice = indice;

        // Verificamos que el audioSource esté activo y habilitado antes de reproducir
        if (audioSource != null && audioSource.isActiveAndEnabled)
        {
            audioSource.PlayOneShot(frasesTorrente[indice]);
        }
    }

    void DetenerFrases()
    {
        // Cancela la repetición para que deje de contar el tiempo
        CancelInvoke("ReproducirFraseRandom");

        if (audioSource != null)
        {
            audioSource.Stop();
        }
        Debug.Log("Frases detenidas por modo Combate");
    }

    // NUEVA FUNCIÓN para reactivar
    void ReanudarFrases()
    {
        // IsInvoking comprueba si ya se está ejecutando.
        // Esto evita que  entre escenas seguras se duplique la velocidad de las frases.
        if (!IsInvoking("ReproducirFraseRandom"))
        {
            InvokeRepeating("ReproducirFraseRandom", tiempoEntreFrases, tiempoEntreFrases);
            Debug.Log("Frases reactivadas");
        }
    }
}