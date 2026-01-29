using System.Collections;
using TMPro;
using UnityEngine;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI LevelText;
    [SerializeField] HPBar hpBar;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        NameText.text = pokemon.Base.Name;
        LevelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
    }
    public IEnumerator UpdateHP()
    {
       yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }
}
