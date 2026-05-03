using UnityEngine;
[CreateAssetMenu(menuName = "Items/Crear nueva pokeball")]

public class PokeballItem : ItemBase
{
    [SerializeField] float cathRateModifier = 1f;
    public override bool Use(Pokemon pokemon)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Combate")
        {
            return false; // No funciona fuera de combate
        }
        return true;
    }
    public float CathRateModifier => cathRateModifier;
}
