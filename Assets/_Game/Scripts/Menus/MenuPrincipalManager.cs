using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems;    

public class MenuPrincipalManager : MonoBehaviour
{
    public GameObject primerBoton;

    void Start()
    {
        // Selecciona el primer botón al iniciar el menú
        EventSystem.current.SetSelectedGameObject(primerBoton);
    }

    // 1. Botón JUGAR
    public void BotonJugar()
    {
        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Partidas");
    }

    // 2. Botón OPCIONESz
    public void BotonOpciones()
    {
        //Guardar esta escena
        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Opciones");
    }

    // 3. Botón CRÉDITOS
    public void BotonCreditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    // 4. Botón SALIR
    public void BotonSalir()
    {
        Debug.Log("Saliendo del juego..."); // Para ver que funciona en el editor
        Application.Quit(); // Cierra el juego (solo funciona en el .exe final)
    }

    // Seguridad: Si haces clic fuera, vuelve a seleccionar el botón
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(primerBoton);
        }
    }
}