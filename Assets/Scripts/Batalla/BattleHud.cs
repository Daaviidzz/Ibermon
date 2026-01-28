using TMPro;
using UnityEngine;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI LevelText;
    [SerializeField] HPBar hpBar;

    public void SetData(Pokemons pokemons)
    {
        
        NameText.text = pokemons.Base.Name;
        LevelText.text = "Lvl " + pokemons.Level;
        hpBar.SetHP((float)pokemons.HP / pokemons.MaxHp);
    }
}
