using UnityEngine;
using System.Collections.Generic;

public class TorrenteFrases : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> frasesTorrente = new List<AudioClip>();
    public float tiempoEntreFrases = 300f;
    private int ultimoIndice = -1;

    void Awake()
    {
        // Si no lo has asignado en el Inspector, lo busca automáticamente
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Si aún no existe, lo creamos en el momento
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        InvokeRepeating("ReproducirFraseRandom", tiempoEntreFrases, tiempoEntreFrases);
    }

    void ReproducirFraseRandom()
    {
        if (frasesTorrente.Count == 0)
        {
            Debug.LogWarning("No hay frases de Torrente asignadas.");
            return;
        }

        int indice = Random.Range(0, frasesTorrente.Count);

        if (indice == ultimoIndice && frasesTorrente.Count > 1)
        {
            indice = (indice + 1) % frasesTorrente.Count;
        }

        ultimoIndice = indice;
        audioSource.PlayOneShot(frasesTorrente[indice]);
    }
}