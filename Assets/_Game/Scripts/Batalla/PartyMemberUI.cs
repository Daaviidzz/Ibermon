using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI LevelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highligthedColor;
    Pokemon _pokemon;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();

        _pokemon.OnHpChanged += UpdateData;
    }
    void UpdateData()
    {
        NameText.text = _pokemon.Base.Name;
        LevelText.text = "Lvl " + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            NameText.color = highligthedColor; 
        }else
            NameText.color=Color.black;
    }
}
