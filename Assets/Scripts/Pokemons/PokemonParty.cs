using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    private  void Start()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    // Retorna el primer pokemon con HP > 0
    public Pokemon GetHealtyPokemon()
    {
      return pokemons.Where(p => p.HP > 0).FirstOrDefault();

    }
}
