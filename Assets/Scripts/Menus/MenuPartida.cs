using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuPartida : MonoBehaviour
{
    //variable que hace referencia al script
    public CrearYPosicionarPlayer posicionarPlayer;
    public GameObject primerBoton;

    void Start()
    {
        // Selecciona el primer botón al iniciar el menú
        EventSystem.current.SetSelectedGameObject(primerBoton);
    }

    public void BotonCargar()
    {
      
        List<Pokemon> equipoCargado = SistemGuardadoPokemon.CargarPartida();
        posicionarPlayer.crearEInstanciarPersonaje();
    }
    public void BotonNueva()
    {
        List<Pokemon> equipoNuevo = SistemGuardadoPokemon.NuevaPartida();
        posicionarPlayer.crearEInstanciarPersonaje();
    }


}
