using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Prefab del jugador")]
    public GameObject jugadorPrefab;

    [Header("Posición inicial")]
    public Vector2 spawnPosition = Vector2.zero;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SpawnJugador();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    void SpawnJugador()
    {
        if (GameObject.FindWithTag("Player") == null)
        {
            GameObject jugador = Instantiate(jugadorPrefab, spawnPosition, Quaternion.identity);
            jugador.name = "Jugador";
        }
    }
}
