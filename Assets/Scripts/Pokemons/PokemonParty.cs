using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    //  Hacemos que la propiedad devuelva la lista del inspector
    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
    }

    private void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    // Retorna el primer pokemon con HP > 0
    public Pokemon GetHealtyPokemon()
    {
        // Aseguramos que la lista no sea nula para evitar otro error
        if (pokemons == null) return null;

        return pokemons.Where(p => p.HP > 0).FirstOrDefault();
    }

    public bool AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6) {
            pokemons.Add(newPokemon);
            return true;
        }
        else
            return false;
    }
    public void HealAllPokemonsInParty()
    {
        // Iteramos por cada pokemon en la lista y le decimos que se cure individualmente
        foreach (var pokemon in pokemons)
        {
            pokemon.ResetHealth();
        }

        
    }
}