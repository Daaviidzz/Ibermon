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

    // Referencia privada al objeto Pokémon cuyos datos estamos mostrando.
    
    Pokemon _pokemon;

    // Inicializa el HUD con los datos de un Pokémon específico.
    // Se llama al iniciar la batalla o cuando se cambia de Pokémon.
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon; // Guardamos la referencia para uso futuro.

        // Asignamos el nombre y el nivel a los textos de la UI.
        NameText.text = pokemon.Base.Name;
        LevelText.text = "Lvl " + pokemon.Level;

        // Ajustamos la barra de vida a su estado inicial.
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
    }

    // Corrutina para actualizar la barra de vida con una animación suave.
    // Se debe llamar después de que el Pokémon reciba daño o se cure.
    public IEnumerator UpdateHP()
    {
        // Llamamos a SetHPSmooth del script HPBar, que se encargará de bajar la barra poco a poco visualmente.
        // 'yield return' significa que el código esperará aquí hasta que la animación de la barra termine.
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }
}
