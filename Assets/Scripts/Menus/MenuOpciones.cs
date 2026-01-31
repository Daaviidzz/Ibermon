using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuOpciones : MonoBehaviour
{
    //boton volver
    public Button botonVolver;

    //desbloqueamos el cursor desde el principio del juego
    private void Awake()
    {
        //Para que el cursor se pueda mover
        Cursor.lockState = CursorLockMode.None;
        //Para que se vea
        Cursor.visible = true;

        botonVolver.onClick.AddListener(() =>
        {

            //Para que el cursor se quede en el medio, es basicamente una forma de bloquearlo
            Cursor.lockState = CursorLockMode.Locked;
            //Y ahora le quitamos la visibilidad
            Cursor.visible = false;

            //Le asignamos la posición al jugador
            JugadorSpawn.posicion = GuardarPosicionAnterior.posicionAnterior;
            //Cambiamos de escena
            SceneManager.LoadScene(GuardarPosicionAnterior.escenaAnterior);
        });
    }

}
