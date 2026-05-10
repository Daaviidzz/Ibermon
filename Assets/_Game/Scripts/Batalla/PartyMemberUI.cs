using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI LevelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Image pokemonIcon;

    [SerializeField] Color highligthedColor;
    Pokemon _pokemon;
    Image fondo;
    bool seleccionado;

    private void Awake()
    {
        fondo = GetComponent<Image>();
    }

    public void SetData(Pokemon pokemon)
    {
        if (_pokemon != null)
            _pokemon.OnHpChanged -= UpdateData;

        _pokemon = pokemon;
        if (_pokemon == null) return;

        PrepararIcono();
        UpdateData();

        _pokemon.OnHpChanged += UpdateData;
    }

    void UpdateData()
    {
        if (_pokemon == null) return;
        if (this == null || !gameObject) return;

        if (NameText != null)
            NameText.text = seleccionado ? "> " + _pokemon.Base.Name : _pokemon.Base.Name;

        if (LevelText != null)
            LevelText.text = "Lvl " + _pokemon.Level;

        if (hpBar != null)
            hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp);

        if (pokemonIcon != null)
        {
            var sprite = GetPokemonSprite();
            pokemonIcon.sprite = sprite;
            pokemonIcon.enabled = sprite != null;
            pokemonIcon.gameObject.SetActive(true);
            pokemonIcon.color = Color.white;

            if (sprite == null)
                Debug.LogWarning(_pokemon.Base.Name + " no tiene sprite asignado.");
        }
    }

    public void SetSelected(bool selected)
    {
        seleccionado = selected;
        if (NameText == null) return;

        if (selected)
        {
            NameText.color = highligthedColor;
            NameText.text = "> " + NameText.text.TrimStart('>', ' ');
            if (fondo != null)
                fondo.color = new Color(0.92f, 0.96f, 1f, 1f);
        }
        else
        {
            NameText.color = Color.black;
            NameText.text = NameText.text.TrimStart('>', ' ');
            if (fondo != null)
                fondo.color = new Color(1f, 1f, 1f, 0.94f);
        }
    }

    public void RefreshDisplay() => UpdateData();

    private void OnDestroy()
    {
        if (_pokemon != null)
            _pokemon.OnHpChanged -= UpdateData;
    }

    // Solo crea el Image si no existe. Tamaño/posición van en el prefab.
    private void PrepararIcono()
    {
        if (pokemonIcon != null) return;

        var iconTransform = transform.Find("PokemonIcon") as RectTransform;
        if (iconTransform == null)
        {
            var obj = new GameObject("PokemonIcon", typeof(RectTransform), typeof(Image));
            iconTransform = obj.GetComponent<RectTransform>();
            iconTransform.SetParent(transform, false);
        }

        pokemonIcon = iconTransform.GetComponent<Image>();
        pokemonIcon.preserveAspect = true;
        pokemonIcon.raycastTarget = false;
        pokemonIcon.color = Color.white;
        pokemonIcon.transform.SetAsLastSibling();
    }

    private Sprite GetPokemonSprite()
    {
        if (_pokemon == null) return null;
        if (_pokemon.FrontSprite != null) return _pokemon.FrontSprite;
        if (_pokemon.Base.FrontSprite != null) return _pokemon.Base.FrontSprite;
        if (_pokemon.BackSprite != null) return _pokemon.BackSprite;
        return _pokemon.Base.BackSprite;
    }
}