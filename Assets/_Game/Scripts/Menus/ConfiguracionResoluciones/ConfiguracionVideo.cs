using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ConfiguracionVideo : MonoBehaviour
{
    // Referencia al Dropdown de las resoluciones
    public TMP_Dropdown dropdownResoluciones;

    // Toggle para pantalla completa (opcional, si lo tienes en la UI)
    public UnityEngine.UI.Toggle togglePantallaCompleta;

    // Array para guardar las resoluciones disponibles
    Resolution[] resoluciones;

    // Detectar si estamos en móvil o PC
    private bool esMovil;

    void Awake()
    {
        comprobacionInicialParteMovil();

        // En móvil, desactivar el dropdown porque no tiene sentido cambiar resolución
        if (esMovil && dropdownResoluciones != null)
        {
            dropdownResoluciones.gameObject.SetActive(false);

            // También ocultar el toggle de pantalla completa si existe
            if (togglePantallaCompleta != null)
            {
                togglePantallaCompleta.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        // Solo configurar resoluciones en PC
        if (!esMovil && dropdownResoluciones != null)
        {
            ConfigurarResoluciones();

            // IMPORTANTE: Conectar el evento del dropdown por código
            // Esto asegura que funcione siempre
            dropdownResoluciones.onValueChanged.AddListener(CambiarResolucion);

            Debug.Log("Dropdown de resoluciones configurado correctamente");
        }

        // Configurar el toggle de pantalla completa si existe
        if (!esMovil && togglePantallaCompleta != null)
        {
            togglePantallaCompleta.onValueChanged.AddListener(PantallaCompleta);
        }
    }

    private void ConfigurarResoluciones()
    {
        // Obtener las resoluciones disponibles del monitor
        resoluciones = Screen.resolutions;
        dropdownResoluciones.ClearOptions();

        // Convertir las resoluciones a una lista de texto
        List<string> opciones = new List<string>();
        int resolucionActual = 0;

        // Llenar la lista de opciones y encontrar la resolución actual
        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            // Guardar cuál es la resolución que tiene el monitor ahora mismo
            if (resoluciones[i].width == Screen.width &&
                resoluciones[i].height == Screen.height)
            {
                resolucionActual = i;
            }
        }

        // Llenar el Dropdown y marcar la actual
        dropdownResoluciones.AddOptions(opciones);
        dropdownResoluciones.value = resolucionActual;
        dropdownResoluciones.RefreshShownValue();

        Debug.Log($"Resoluciones cargadas: {opciones.Count}. Resolucion actual: {resoluciones[resolucionActual].width}x{resoluciones[resolucionActual].height}");

        // Si existe el toggle de pantalla completa, configurarlo
        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.isOn = Screen.fullScreen;
        }
    }

    // Función que se activa al cambiar el Dropdown
    public void CambiarResolucion(int indiceResolucion)
    {
        Debug.Log($"CambiarResolucion llamado con indice: {indiceResolucion}");

        // Solo funciona en PC
        if (esMovil)
        {
            Debug.Log("Cambio de resolucion ignorado - estamos en movil");
            return;
        }

        // Verificar que el índice sea válido
        if (resoluciones == null || indiceResolucion < 0 || indiceResolucion >= resoluciones.Length)
        {
            Debug.LogError($"Indice de resolucion invalido: {indiceResolucion}");
            return;
        }

        // Obtener la resolución seleccionada y aplicarla
        Resolution resolucion = resoluciones[indiceResolucion];

        Debug.Log($"Intentando cambiar a: {resolucion.width}x{resolucion.height}, Pantalla completa: {Screen.fullScreen}");

        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);

        Debug.Log($"Resolucion aplicada. Resolucion actual de Screen: {Screen.width}x{Screen.height}");
    }

    // Función para activar o desactivar el modo pantalla completa
    public void PantallaCompleta(bool esCompleta)
    {
        // Solo funciona en PC
        if (esMovil) return;

        Screen.fullScreen = esCompleta;

        Debug.Log("Pantalla completa: " + (esCompleta ? "Activada" : "Desactivada"));
    }

    //Parte movil inicial
    private void comprobacionInicialParteMovil()
    {
        // Detectar la plataforma
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif
    }

    // Método para limpiar el listener cuando se destruye el objeto
    private void OnDestroy()
    {
        if (dropdownResoluciones != null)
        {
            dropdownResoluciones.onValueChanged.RemoveListener(CambiarResolucion);
        }

        if (togglePantallaCompleta != null)
        {
            togglePantallaCompleta.onValueChanged.RemoveListener(PantallaCompleta);
        }
    }
}