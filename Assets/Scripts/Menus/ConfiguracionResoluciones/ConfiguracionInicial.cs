using UnityEngine;

public class ConfiguracionInicial : MonoBehaviour
{
    // Este script se debe colocar en la primera escena del juego (portada)

    [Header("Configuración de resolución inicial")]
    [Tooltip("Ancho de la ventana al iniciar el juego")]
    public int anchoInicial = 800;

    [Tooltip("Alto de la ventana al iniciar el juego")]
    public int altoInicial = 600;

    [Tooltip("Iniciar en pantalla completa")]
    public bool pantallaCompletaInicial = false;

    private void Awake()
    {
        // Solo aplicar en PC (no en móvil)
#if !UNITY_ANDROID && !UNITY_IOS

        // Establecer resolución inicial
        Screen.SetResolution(anchoInicial, altoInicial, pantallaCompletaInicial);

        Debug.Log($"Resolucion inicial establecida: {anchoInicial} x {altoInicial}");

#endif
    }
}