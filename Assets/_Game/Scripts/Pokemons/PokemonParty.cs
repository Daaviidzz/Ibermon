using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;
    [SerializeField] bool esEquipoJugador = false;
    private bool esBatallaTemp = false;
    public event Action OnUpdated;

    //  Hacemos que la propiedad devuelva la lista del inspector
    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
    }

    private void Start()
    {
        if (esBatallaTemp) return;
        if (esEquipoJugador)
        {
            CargarEquipoGuardado(); // extraemos la lógica a un método
        }
    }

    public void CargarEquipoGuardado()
    {
        if (SistemGuardadoPokemon.HayDatosGuardados())
        {
            List<Pokemon> equipoCargado = SistemGuardadoPokemon.CargarEquipo();
            if (equipoCargado != null && equipoCargado.Count > 0)
            {
                pokemons = equipoCargado;
                Debug.Log("Equipo cargado desde guardado");
            }
        }
    }
    public void SetPokemonsForBattle(List<Pokemon> pokemonsEntrenador)
    {
        esBatallaTemp = true;
        pokemons = pokemonsEntrenador;
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    private void OnDestroy()
    {
        // IMPORTANTE: solo guardar si es el jugador, no el temporal del entrenador
        if (gameObject.CompareTag("Player") && pokemons != null && pokemons.Count > 0)
        {
            SistemGuardadoPokemon.GuardarEquipo(pokemons);
        }
    }

    private void OnApplicationQuit()
    {
        if (gameObject.CompareTag("Player") && pokemons != null && pokemons.Count > 0)
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
            OnUpdated?.Invoke();
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
            pokemon.CureStatus();
        }

        // Guardar después de curar
        SistemGuardadoPokemon.GuardarEquipo(pokemons);
    }
    public static PokemonParty GetPlayerParty()
    {
        return GameObject.FindGameObjectWithTag("Player").GetComponent<PokemonParty>();
    }

    
}