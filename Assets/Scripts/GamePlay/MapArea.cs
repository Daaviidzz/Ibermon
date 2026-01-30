using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//Clase que representa un area del mapa donde pueden aparecer pokemons salvajes
public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    //Metodo para obtener un pokemon salvaje aleatorio del area
    public Pokemon GetRandomWildPokemon()
    {
        int randomIndex = Random.Range(0, wildPokemons.Count);
        var wildPokemon= wildPokemons[randomIndex];
        wildPokemon.Init();
        return wildPokemon;
    }
}
