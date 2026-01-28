using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerCreditos : MonoBehaviour
{
    // Tiempo en segundos antes de volver al menú principal
    public float tiempoEspera = 6f;
    public string nombreEscenaMenu = "MenuPrincipal"; 

    void Start()
    {
        // Esto le dice a Unity: "Ejecuta la función VolverAlMenu dentro de 6 segundos"
        Invoke("VolverAlMenu", tiempoEspera);
    }

    void VolverAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    // Si el jugador presiona cualquier tecla, volvemos al menú inmediatamente
    void Update()
    {
        if (Input.anyKeyDown)
        {
            VolverAlMenu();
        }
    }
}