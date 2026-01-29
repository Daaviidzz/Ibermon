using UnityEngine;
using System.Collections.Generic;

public class TorrenteFrases : MonoBehaviour
{
    // Este AudioSource es el que va a reproducir las frases.
    // Puede ser uno dedicado solo a Torrente o el mismo que uses para efectos.
    public AudioSource audioSource;

    // Aquí metes todas las frases que quieras que diga Torrente.
    // Simplemente arrastras los AudioClips al inspector y listo.
    public List<AudioClip> frasesTorrente = new List<AudioClip>();

    // Cada cuántos segundos quieres que hable.
    // 300 segundos = 5 minutos. Lo puedes cambiar cuando quieras.
    public float tiempoEntreFrases = 300f;

    // Esto es solo para evitar que repita la misma frase dos veces seguidas.
    private int ultimoIndice = -1;

    void Start()
    {
        // InvokeRepeating es como decirle a Unity:
        // "Oye, llama a esta función dentro de X segundos, y luego repítela cada X segundos".
        // Es súper cómodo para cosas que se repiten solas.
        InvokeRepeating("ReproducirFraseRandom", tiempoEntreFrases, tiempoEntreFrases);
    }

    void ReproducirFraseRandom()
    {
        // Si por lo que sea no has metido frases, mejor no hacer nada.
        if (frasesTorrente.Count == 0)
        {
            Debug.LogWarning("No hay frases de Torrente asignadas.");
            return;
        }

        // Elegimos una frase aleatoria.
        // Pero vamos a evitar que salga la misma dos veces seguidas,
        // porque queda feo que Torrente repita como un loro.
        int indice = Random.Range(0, frasesTorrente.Count);

        // Si por casualidad toca la misma frase que antes, volvemos a tirar.
        // Esto evita repeticiones tontas.
        if (indice == ultimoIndice && frasesTorrente.Count > 1)
        {
            indice = (indice + 1) % frasesTorrente.Count;
        }

        // Guardamos cuál fue la última frase para evitar repeticiones.
        ultimoIndice = indice;

        // Y ahora sí: reproducimos la frase.
        // PlayOneShot es perfecto para sonidos cortos y puntuales.
        audioSource.PlayOneShot(frasesTorrente[indice]);

        // Y ya está. Unity se encargará de volver a llamar a esta función
        // dentro de X minutos sin que tú tengas que hacer nada más.
    }
}
