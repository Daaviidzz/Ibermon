using UnityEngine;
[CreateAssetMenu(menuName = "Items/Crear nueva pokeball")]

public class PokeballItem : ItemBase
{
  public override bool Use(Pokemon pokemon)
    {
        return true; // Retorna true para indicar que el item se ha "usado" correctamente, aunque no tenga un efecto directo aquí.
    }
}
