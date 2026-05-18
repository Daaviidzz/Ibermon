using System.Collections.Generic;
using ApiRest.Models;
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
        return CrearPokemonSalvajeDesdeApi(wildPokemon) ?? PrepararPokemonLocal(wildPokemon);
    }

    private Pokemon PrepararPokemonLocal(Pokemon wildPokemon)
    {
        wildPokemon.Init();
        return wildPokemon;
    }

    private Pokemon CrearPokemonSalvajeDesdeApi(Pokemon pokemonLocal)
    {
        if (CatalogoCache.Instance == null || !CatalogoCache.Instance.EstaListo)
            return null;

        if (pokemonLocal?.Base == null)
            return null;

        var catalogo = CatalogoCache.Instance;
        int ibermonCatalogoId = catalogo.GetIbermonNumero(pokemonLocal.Base.Name);
        if (ibermonCatalogoId <= 0)
            return null;

        var pokemonTemporal = new Pokemon(pokemonLocal.Base, pokemonLocal.Level);
        var movimientos = new List<MovimientoAprendido>();

        if (pokemonTemporal.Moves != null)
        {
            foreach (var move in pokemonTemporal.Moves)
            {
                int numeroMovimiento = catalogo.GetMovimientoNumero(move.Base.Name);
                if (numeroMovimiento > 0)
                    movimientos.Add(new MovimientoAprendido { numero = numeroMovimiento, pp = move.PP });
            }
        }

        var ibermon = new IbermonJugador
        {
            ibermon_catalogo_id = ibermonCatalogoId,
            nivel = pokemonTemporal.Level,
            experiencia = pokemonTemporal.Exp,
            hp_actual = pokemonTemporal.MaxHp,
            ubicacion = "equipo",
            movimientos_aprendidos = movimientos,
        };

        return IbermonConverter.ToPokemon(ibermon, catalogo);
    }
}
