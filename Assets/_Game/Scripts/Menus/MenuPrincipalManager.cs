using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.EventSystems;    

public class MenuPrincipalManager : MonoBehaviour
{
    public GameObject primerBoton;

    void Start()
    {
        // Selecciona el primer boton al iniciar el menu
        if (EventSystem.current != null && primerBoton != null)
        {
            EventSystem.current.SetSelectedGameObject(primerBoton);
        }
    }

    // 1. Boton JUGAR
    public void BotonJugar()
    {
        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Partidas");
    }

    // 2. Boton OPCIONES
    public void BotonOpciones()
    {
        //Guardar esta escena
        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("Opciones");
    }

    // 3. Boton CREDITOS
    public void BotonCreditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    // 4. Boton SALIR
    public void BotonSalir()
    {
        Debug.Log("Saliendo del juego..."); // Para ver que funciona en el editor
        Application.Quit(); // Cierra el juego (solo funciona en el .exe final)
    }

    // Seguridad: Si haces clic fuera, vuelve a seleccionar el boton
    void Update()
    {
        if (EventSystem.current != null && primerBoton != null && EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(primerBoton);
        }
    }
}
