using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoVolverEscena : MonoBehaviour
{
    [SerializeField] private float segundosEspera = 5f;

    private void Start()
    {
        Invoke("VolverAEscenaAnterior", segundosEspera);
    }

    private void VolverAEscenaAnterior()
    {
        // Volver a la escena que estaba guardada en JugadorSpawn
        if (!string.IsNullOrEmpty(JugadorSpawn.escenaAnterior))
        {
            SceneManager.LoadScene(JugadorSpawn.escenaAnterior);
        }
        else
        {
            Debug.LogWarning("No hay escena anterior guardada");
        }
    }
}