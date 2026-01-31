using UnityEngine;

public class Interruptor : MonoBehaviour
{
    void Start()
    {
        // Llamamos al script que YA TIENES para que pare la música vieja
        if (ControlMusica.instance != null)
        {
            ControlMusica.instance.PararMusicaMenu();
        }
    }
}