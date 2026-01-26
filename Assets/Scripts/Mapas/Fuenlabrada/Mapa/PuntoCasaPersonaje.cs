using UnityEngine;
using UnityEngine.SceneManagement;

public class PuntoCasaPersonaje : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(1);
        }
    }
}
