using System.Collections.Generic;
using ApiRest.Models;
using UnityEngine;

// Los nombres de los assets en Resources/Pokemons/ y Resources/Moves/ tienen que coincidir
// exactamente con el campo "nombre" del catálogo de la API o los ibermon no cargan.
public static class IbermonConverter
{
    private static Dictionary<string, PokemonBase> _pokemonBases;
    private static Dictionary<string, MoveBase>    _moveBases;

    private static void EnsureResourcesLoaded()
    {
        if (_pokemonBases != null) return;

        _pokemonBases = new Dictionary<string, PokemonBase>();
        _moveBases    = new Dictionary<string, MoveBase>();

        foreach (var pb in Resources.LoadAll<PokemonBase>("Pokemons"))
        {
            if (pb == null) continue;
            _pokemonBases[pb.Name] = pb;

            if (pb.LearnableMoves == null) continue;
            foreach (var lm in pb.LearnableMoves)
            {
                if (lm?.MoveBase != null && !_moveBases.ContainsKey(lm.MoveBase.Name))
                    _moveBases[lm.MoveBase.Name] = lm.MoveBase;
            }
        }

        foreach (var mb in Resources.LoadAll<MoveBase>("Moves"))
        {
            if (mb != null && !_moveBases.ContainsKey(mb.Name))
                _moveBases[mb.Name] = mb;
        }

        Debug.Log($"[IbermonConverter] PokemonBase: {_pokemonBases.Count} | MoveBase: {_moveBases.Count}");
    }

    public static Pokemon ToPokemon(IbermonJugador ib, CatalogoCache catalogo)
    {
        EnsureResourcesLoaded();

        string nombre = catalogo.GetIbermonNombre(ib.ibermon_catalogo_id);
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogError($"[IbermonConverter] ibermon id={ib.ibermon_catalogo_id} no encontrado en catálogo");
            return null;
        }

        if (!_pokemonBases.TryGetValue(nombre, out var pBase))
        {
            Debug.LogError($"[IbermonConverter] No existe PokemonBase '{nombre}' en Resources/Pokemons/");
            return null;
        }

        var pokemon = new Pokemon(pBase, ib.nivel);
        pokemon.Init();

        pokemon.HP  = Mathf.Clamp(ib.hp_actual, 0, pokemon.MaxHp);
        pokemon.Exp = ib.experiencia;

        // Restaurar movimientos guardados si los hay
        if (ib.movimientos_aprendidos != null && ib.movimientos_aprendidos.Count > 0)
        {
            var movesRestaurados = new List<Move>();
            foreach (var movAprendido in ib.movimientos_aprendidos)
            {
                string movNombre = catalogo.GetMovimientoNombre(movAprendido.numero);
                if (movNombre == null)
                {
                    Debug.LogWarning($"[IbermonConverter] Movimiento número {movAprendido.numero} no está en el catálogo.");
                    continue;
                }
                if (_moveBases.TryGetValue(movNombre, out var mb))
                {
                    var move = new Move(mb);
                    move.PP = movAprendido.pp;
                    movesRestaurados.Add(move);
                }
                else
                    Debug.LogWarning($"[IbermonConverter] No existe MoveBase '{movNombre}' en Resources/Moves/");
            }

            if (movesRestaurados.Count > 0)
                pokemon.Moves = movesRestaurados;
        }

        return pokemon;
    }

    public static List<Pokemon> ToPokemons(List<IbermonJugador> equipo, CatalogoCache catalogo)
    {
        var result = new List<Pokemon>();
        foreach (var ib in equipo)
        {
            var p = ToPokemon(ib, catalogo);
            if (p != null) result.Add(p);
        }
        return result;
    }

    public static IbermonJugadorActualizarRequest ToActualizarRequest(Pokemon pokemon, CatalogoCache catalogo)
    {
        var movimientos = new List<MovimientoAprendido>();
        if (pokemon.Moves != null)
        {
            foreach (var move in pokemon.Moves)
            {
                int num = catalogo.GetMovimientoNumero(move.Base.Name);
                if (num > 0)
                    movimientos.Add(new MovimientoAprendido { numero = num, pp = move.PP });
            }
        }

        return new IbermonJugadorActualizarRequest
        {
            nivel                  = pokemon.Level,
            experiencia            = pokemon.Exp,
            hp_actual              = pokemon.HP,
            movimientos_aprendidos = movimientos,
        };
    }

    public static int GetCatalogoId(Pokemon pokemon, CatalogoCache catalogo)
    {
        return catalogo.GetIbermonNumero(pokemon.Base.Name);
    }

    public static void InvalidarCache()
    {
        _pokemonBases = null;
        _moveBases    = null;
    }
}
