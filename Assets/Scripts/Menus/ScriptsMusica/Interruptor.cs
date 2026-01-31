using UnityEngine;

public class Interruptor : MonoBehaviour
{
    private static bool musicaMenuParada = false;

    void Start()
    {
        // Solo para la música del menú UNA VEZ
        if (!musicaMenuParada && ControlMusica.instance != null)
        {
            ControlMusica.instance.PararMusicaMenu();
            musicaMenuParada = true;
        }
    }
}