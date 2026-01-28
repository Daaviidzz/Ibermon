using UnityEngine;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    // ============================
    //  PANELES DEL MENÚ
    // ============================
    // Estos son simplemente los paneles que vas a activar o desactivar
    // según el usuario navegue por el menú. Nada raro.
    public GameObject panelOpciones;
    public GameObject panelControles;


    // ============================
    //  SLIDER DEL VOLUMEN
    // ============================
    // El slider que controla el volumen general del juego.
    // Unity ya trae AudioListener.volume, así que no hay que inventar nada.
    public Slider sliderVolumen;


    // ============================
    //  DROPDOWN DE RESOLUCIONES
    // ============================
    // Aquí guardamos el dropdown y la lista de resoluciones que detecta Unity.
    public Dropdown dropdownResoluciones;
    private Resolution[] resoluciones;


    void Start()
    {
        // ============================
        //  CONFIGURAR EL SLIDER
        // ============================
        // Ponemos el slider en el valor actual del volumen.
        // Así, si el jugador vuelve al menú, no se resetea.
        sliderVolumen.value = AudioListener.volume;

        // Cada vez que el jugador mueva el slider, llamamos a CambiarVolumen.
        sliderVolumen.onValueChanged.AddListener(CambiarVolumen);


        // ============================
        //  CONFIGURAR EL DROPDOWN
        // ============================
        // Unity nos da todas las resoluciones que soporta el monitor.
        resoluciones = Screen.resolutions;

        // Limpiamos el dropdown por si tenía cosas de antes.
        dropdownResoluciones.ClearOptions();

        // Rellenamos el dropdown con las resoluciones disponibles.
        foreach (Resolution r in resoluciones)
        {
            dropdownResoluciones.options.Add(
                new Dropdown.OptionData(r.width + " x " + r.height)
            );
        }

        // Cuando el jugador elija una resolución, llamamos a CambiarResolucion.
        dropdownResoluciones.onValueChanged.AddListener(CambiarResolucion);


        // ============================
        //  MOSTRAR EL PANEL CORRECTO
        // ============================
        // Al entrar al menú, queremos ver las opciones, no los controles.
        panelOpciones.SetActive(true);
        panelControles.SetActive(false);
    }


    // ============================
    //  CAMBIAR VOLUMEN
    // ============================
    // Esto es lo más simple del mundo: cambiamos el volumen global del juego.
    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
    }


    // ============================
    //  CAMBIAR RESOLUCIÓN
    // ============================
    // Aquí simplemente cogemos la resolución que ha elegido el jugador
    // y se la pasamos a Unity. FullScreen se mantiene como esté.
    public void CambiarResolucion(int indice)
    {
        Resolution r = resoluciones[indice];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }


    // ============================
    //  NAVEGAR ENTRE PANELES
    // ============================
    // Esto es literalmente encender un panel y apagar otro.
    // No tiene más misterio.
    public void AbrirControles()
    {
        panelOpciones.SetActive(false);
        panelControles.SetActive(true);
    }

    public void VolverOpciones()
    {
        panelControles.SetActive(false);
        panelOpciones.SetActive(true);
    }
}
