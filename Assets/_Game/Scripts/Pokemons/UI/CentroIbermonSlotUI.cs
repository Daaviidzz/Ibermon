using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CentroIbermonSlotUI : MonoBehaviour
{
    [SerializeField] Image sprite;
    [SerializeField] TextMeshProUGUI nombreText;
    [SerializeField] TextMeshProUGUI nivelText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] Image hpFill;
    [SerializeField] Image resaltado;
    [SerializeField] Button boton;

    public string IbermonId { get; private set; }
    public string Lado { get; private set; }
    public bool TieneIbermon { get; private set; }

    private CentroIbermonUI _padre;
    private IbermonJugador _ibermon;

    private void Awake()
    {
        if (boton == null)
            boton = GetComponent<Button>();
    }

    public void Configurar(IbermonJugador ibermon, string lado, CentroIbermonUI padre)
    {
        _ibermon = ibermon;
        _padre = padre;
        Lado = lado;
        TieneIbermon = ibermon != null;
        IbermonId = TieneIbermon ? ibermon.id : null;

        if (TieneIbermon)
            PintarIbermon();
        else
            PintarHuecoVacio();

        SetResaltado(false);

        if (boton != null)
        {
            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(() => _padre?.OnSlotClick(this));
        }
    }

    public void SetResaltado(bool activo)
    {
        if (resaltado != null)
            resaltado.enabled = activo;
    }

    private void PintarHuecoVacio()
    {
        if (nombreText != null)
            nombreText.text = "(vacio)";

        if (nivelText != null)
            nivelText.enabled = false;

        if (hpText != null)
            hpText.enabled = false;

        if (hpFill != null)
        {
            hpFill.fillAmount = 0f;
            hpFill.transform.parent.gameObject.SetActive(false);
        }

        if (sprite != null)
        {
            sprite.sprite = null;
            sprite.enabled = false;
        }
    }

    private void PintarIbermon()
    {
        CatalogoCache catalogo = CatalogoCache.Instance;
        if (catalogo == null)
        {
            Debug.LogError("[CentroIbermonSlotUI] CatalogoCache no disponible.");
            return;
        }

        string nombreCatalogo = catalogo.GetIbermonNombre(_ibermon.ibermon_catalogo_id);
        string nombre = string.IsNullOrWhiteSpace(_ibermon.nickname) ? nombreCatalogo : _ibermon.nickname;

        if (nombreText != null)
        {
            nombreText.enabled = true;
            nombreText.text = string.IsNullOrEmpty(nombre) ? "Ibermon" : nombre;
        }

        if (nivelText != null)
        {
            nivelText.enabled = true;
            nivelText.text = $"Lv {_ibermon.nivel}";
        }

        if (hpText != null)
        {
            hpText.enabled = true;
            hpText.text = "HP";
        }

        Pokemon pokemon = IbermonConverter.ToPokemon(_ibermon, catalogo);
        Sprite icono = null;

        if (pokemon != null)
        {
            icono = pokemon.FrontSprite != null
                ? pokemon.FrontSprite
                : pokemon.Base != null ? pokemon.Base.FrontSprite : null;

            PintarVida(pokemon.MaxHp);
        }

        if (icono == null)
            icono = CargarSpriteDirecto(catalogo, _ibermon.ibermon_catalogo_id);

        if (sprite != null)
        {
            sprite.sprite = icono;
            sprite.enabled = icono != null;
            sprite.preserveAspect = true;
        }
    }

    private void PintarVida(int maxHp)
    {
        int hpActual = Mathf.Clamp(_ibermon.hp_actual, 0, maxHp);
        float normalizado = maxHp > 0 ? (float)hpActual / maxHp : 0f;

        if (hpFill != null)
        {
            hpFill.transform.parent.gameObject.SetActive(true);
            hpFill.fillAmount = normalizado;
            hpFill.color = normalizado > 0.5f
                ? new Color(0.18f, 0.86f, 0.27f)
                : normalizado > 0.2f ? new Color(0.95f, 0.78f, 0.18f) : new Color(0.9f, 0.2f, 0.12f);
        }
    }

    private static Sprite CargarSpriteDirecto(CatalogoCache catalogo, int catalogoId)
    {
        Sprite spriteApi = CargarDesdeResources(catalogo.GetSpriteFrontal(catalogoId));
        if (spriteApi != null) return spriteApi;

        Sprite spritePorId = CargarDesdeResources(catalogoId.ToString());
        if (spritePorId != null) return spritePorId;

        return catalogoId > 1000 ? CargarDesdeResources((catalogoId - 1000).ToString()) : null;
    }

    private static Sprite CargarDesdeResources(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        string sinExt = path.EndsWith(".png") ? path[..^4] : path;
        string resourcesPath = $"Sprites/Pokemon/{sinExt}";

        Sprite sprite = Resources.Load<Sprite>(resourcesPath);
        if (sprite != null) return sprite;

        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcesPath);
        return sprites != null && sprites.Length > 0 ? sprites[0] : null;
    }
}
