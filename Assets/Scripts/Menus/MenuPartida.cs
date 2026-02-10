using UnityEngine;

public class MenuPartida : MonoBehaviour
{
    //variable que hace referencia al script
    public CrearYPosicionarPlayer posicionarPlayer;

    public void BotonCargar()
    {
        //creamos el personaje y lo posicionamos
        posicionarPlayer.crearEInstanciarPersonaje();
    }
    public void BotonNueva()
    {
        //llamamos al metodo que borra datos
        //creamos el personaje y lo posicionamos
        posicionarPlayer.crearEInstanciarPersonaje();
    }


}
