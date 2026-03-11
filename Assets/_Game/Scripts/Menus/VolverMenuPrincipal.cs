using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNavegacion : MonoBehaviour
{
    public void IrAMenuPrincipal()
    {
       
        SceneManager.LoadScene("MenuPrincipal");
    }
}