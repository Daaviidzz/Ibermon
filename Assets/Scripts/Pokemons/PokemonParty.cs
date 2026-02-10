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
    {/*
        // Intentar cargar el equipo guardado
        if (SistemGuardadoPokemon.HayDatosGuardados())
        {
           
            List<Pokemon> equipoCargado = SistemGuardadoPokemon.CargarEquipo();
            if (equipoCargado != null && equipoCargado.Count > 0)
            {
                pokemons = equipoCargado;
                Debug.Log("Equipo cargado desde guardado");
            }
        }
        else
        */
        
            // Si no hay guardado, inicializar los Pokémon del inspector
            foreach (var pokemon in pokemons)
            {
                pokemon.Init();
            }
        
    }

    private void OnDestroy()
    {
        // Guardar automáticamente al destruir el objeto (al cerrar el juego o cambiar escena)
        if (pokemons != null && pokemons.Count > 0)
        {
            SistemGuardadoPokemon.GuardarEquipo(pokemons);
        }
    }

    private void OnApplicationQuit()
    {
        // Guardar también al cerrar la aplicación
        if (pokemons != null && pokemons.Count > 0)
        {
            SistemGuardadoPokemon.GuardarEquipo(pokemons);
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
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            // Guardar inmediatamente al capturar un nuevo Pokémon
            SistemGuardadoPokemon.GuardarEquipo(pokemons);
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

        // Guardar después de curar
        SistemGuardadoPokemon.GuardarEquipo(pokemons);
    }
}