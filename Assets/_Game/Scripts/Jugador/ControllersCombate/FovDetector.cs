using UnityEngine;

// Este script va en el GameObject hijo "CampoDeVision"
public class FovDetector : MonoBehaviour
{
    private TrainerController entrenador;

    private void Awake()
    {
        // Sube al padre para coger el TrainerController
        entrenador = GetComponentInParent<TrainerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            entrenador.JugadorDetectado(collision.transform);
        }
    }
}