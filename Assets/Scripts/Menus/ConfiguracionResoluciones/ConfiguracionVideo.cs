using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ConfiguracionVideo : MonoBehaviour
{
    // Referencia al Dropdown de las resoluciones
    public TMP_Dropdown dropdownResoluciones;
    // Array para guardar las resoluciones disponibles
    Resolution[] resoluciones;

    void Start()
    {
        //  Obtener las resoluciones disponibles del monitor
        resoluciones = Screen.resolutions;
        dropdownResoluciones.ClearOptions();

        //  Convertir las resoluciones a una lista de texto
        List<string> opciones = new List<string>();
        int resolucionActual = 0;

        //  Llenar la lista de opciones y encontrar la resolución actual
        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            // Guardar cuál es la resolución que tiene el monitor ahora mismo
            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                resolucionActual = i;
            }
        }

        // 3. Llenar el Dropdown y marcar la actual
        dropdownResoluciones.AddOptions(opciones);
        dropdownResoluciones.value = resolucionActual;
        dropdownResoluciones.RefreshShownValue();
    }

    // 4. Función que se activa al cambiar el Dropdown
    public void CambiarResolucion(int indiceResolucion)
    {
        // Obtener la resolución seleccionada y aplicarla
        Resolution resolucion = resoluciones[indiceResolucion];
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);
    }

    // Función para activar o desactivar el modo pantalla completa
    public void PantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
    }
}