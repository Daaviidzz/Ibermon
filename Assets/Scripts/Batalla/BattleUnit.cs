using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemons Pokemons { get; set; }
    public void Setup()
    {
       
        Pokemons = new Pokemons(_base, level);
        if(isPlayerUnit)
            GetComponentInChildren<Image>().sprite = Pokemons.Base.BackSprite;
        else
            GetComponent<Image>().sprite = Pokemons.Base.FrontSprite;
    }
}
