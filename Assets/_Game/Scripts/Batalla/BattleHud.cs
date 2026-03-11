using DG.Tweening;
using System.Collections;
using TMPro; 
using UnityEngine;

// Esta clase controla la interfaz visual (HUD) de un Pokémon en batalla.
// Se encarga de mostrar el Nombre, Nivel y la Barra de Vida (HP).
//habrá dos instancias de esto: una para el jugador y otra para el enemigo.
public class BattleHud : MonoBehaviour
{
    

    [SerializeField] TextMeshProUGUI NameText; // Texto para el nombre del Pokémon.
    [SerializeField] TextMeshProUGUI LevelText; // Texto para el nivel (ej: "Lvl 15").
    [SerializeField] HPBar hpBar; // Referencia al script 'HPBar' que controla visualmente la barra de vida (el relleno verde).
    [SerializeField] GameObject expBar; // Referencia al objeto Exp
    // Referencia privada al objeto Pokémon cuyos datos estamos mostrando.
    
    Pokemon _pokemon;

    // Inicializa el HUD con los datos de un Pokémon específico.
    // Se llama al iniciar la batalla o cuando se cambia de Pokémon.
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon; // Guardamos la referencia para uso futuro.

        // Asignamos el nombre y el nivel a los textos de la UI.
        NameText.text = pokemon.Base.Name;
        SetLevel();

        // Ajustamos la barra de vida a su estado inicial.
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
        SetExp();
    }

    // Corrutina para actualizar la barra de vida con una animación suave.
    // Se debe llamar después de que el Pokémon reciba daño o se cure.
    public IEnumerator UpdateHP()
    {
        // Llamamos a SetHPSmooth del script HPBar, que se encargará de bajar la barra poco a poco visualmente.
        // 'yield return' significa que el código esperará aquí hasta que la animación de la barra termine.
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }
    public void SetLevel()
    {
        LevelText.text = "Lvl " + _pokemon.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1f, 1f); 

    }
    public IEnumerator SetExpSmooth(bool reset=false)
    {
        if (expBar == null) yield break;

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1f, 1f);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp,1.5f).WaitForCompletion();
    }
    float GetNormalizedExp()
    {
        int currLevelExp= _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level+1);

        float normalizedExp = (float)(_pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }
}
