using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlMenuPrincipal : MonoBehaviour
{
    void Update()
    {
        //Para detectar si pulsa enter
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            //Como esta escena sería la 0 la del menú principal y queremos acceder a la del juego que sería la siguiente
            //la escena a la que queremos acceder será la 1
            SceneManager.LoadScene(1);
        }
            
    }
}
