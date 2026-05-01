using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI LevelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highligthedColor;
    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        // Si ya tenemos un Pokémon, desuscribirse del anterior
        if (_pokemon != null)
        {
            _pokemon.OnHpChanged -= UpdateData;
        }

        _pokemon = pokemon;
        UpdateData();

        // Suscribirse al evento del nuevo Pokémon
        _pokemon.OnHpChanged += UpdateData;
    }

    void UpdateData()
    {
        if (_pokemon == null) return;

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

    // Método para forzar una actualización del display (útil cuando se abre PartyScreen durante batalla)
    public void RefreshDisplay()
    {
        UpdateData();
    }
}
