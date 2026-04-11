using System.Collections.Generic;
using ApiRest.Models;
using UnityEngine;

/// <summary>
/// Clase estática que convierte entre modelos de la API y objetos del juego.
///
/// REQUISITO: Los ScriptableObjects de PokemonBase deben estar en Resources/Pokemons/
/// con nombres que coincidan exactamente con el campo "nombre" del catálogo de la API
/// (ej: "Ignifor", "Aquillo", "Verdino").
///
/// REQUISITO: Los ScriptableObjects de MoveBase deben estar en Resources/Moves/ o
/// referenciados en los LearnableMoves de algún PokemonBase. Sus nombres deben coincidir
/// con el campo "nombre" del catálogo de movimientos (ej: "Placaje", "Ascuas").
/// </summary>
public static class IbermonConverter
{
    // ─── Caché de ScriptableObjects ───────────────────────────────────────────

    private static Dictionary<string, PokemonBase> _pokemonBases;
    private static Dictionary<string, MoveBase>    _moveBases;

    /// <summary>
    /// Carga (si aún no están cargados) todos los ScriptableObjects desde Resources.
    /// Se indexan por su nombre para búsquedas O(1).
    /// </summary>
    private static void EnsureResourcesLoaded()
    {
        if (_pokemonBases != null) return;

        _pokemonBases = new Dictionary<string, PokemonBase>();
        _moveBases    = new Dictionary<string, MoveBase>();

        // ── PokemonBase desde Resources/Pokemons/ ──────────────────────────
        foreach (var pb in Resources.LoadAll<PokemonBase>("Pokemons"))
        {
            if (pb == null) continue;
            _pokemonBases[pb.Name] = pb;

            // Recoger también todos los MoveBase referenciados en sus learnableMoves
            if (pb.LearnableMoves == null) continue;
            foreach (var lm in pb.LearnableMoves)
            {
                if (lm?.MoveBase != null && !_moveBases.ContainsKey(lm.MoveBase.Name))
                    _moveBases[lm.MoveBase.Name] = lm.MoveBase;
            }
        }

        // ── MoveBase desde Resources/Moves/ (si existen como assets sueltos) ─
        foreach (var mb in Resources.LoadAll<MoveBase>("Moves"))
        {
            if (mb != null && !_moveBases.ContainsKey(mb.Name))
                _moveBases[mb.Name] = mb;
        }

        Debug.Log($"[IbermonConverter] PokemonBase cargados: {_pokemonBases.Count} | MoveBase: {_moveBases.Count}");
    }

    // ─── API → Unity ──────────────────────────────────────────────────────────

    /// <summary>
    /// Convierte un IbermonJugador (modelo API) en un Pokemon (objeto de juego).
    /// Busca el PokemonBase por nombre usando el catálogo, y carga los movimientos
    /// guardados. Si no encuentra el PokemonBase devuelve null y loguea un error.
    /// </summary>
    public static Pokemon ToPokemon(IbermonJugador ib, CatalogoCache catalogo)
    {
        EnsureResourcesLoaded();

        // 1. Resolver nombre a partir del id de catálogo
        string nombre = catalogo.GetIbermonNombre(ib.ibermon_catalogo_id);
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogError($"[IbermonConverter] ibermon_catalogo_id={ib.ibermon_catalogo_id} no está en el catálogo. " +
                           "¿Se han cargado los catálogos? ¿El servidor tiene datos?");
            return null;
        }

        // 2. Encontrar el ScriptableObject
        if (!_pokemonBases.TryGetValue(nombre, out var pBase))
        {
            Debug.LogError($"[IbermonConverter] No se encontró PokemonBase '{nombre}' en Resources/Pokemons/. " +
                           "Asegúrate de que el nombre del asset coincide exactamente con el del catálogo.");
            return null;
        }

        // 3. Crear la instancia con Init() (calcula stats, HP máximo, movimientos por nivel)
        var pokemon = new Pokemon(pBase, ib.nivel);
        pokemon.Init();

        // 4. Sobreescribir con los valores guardados en la API
        pokemon.HP  = Mathf.Clamp(ib.hp_actual, 0, pokemon.MaxHp);
        pokemon.Exp = ib.experiencia;

        // 5. Restaurar movimientos guardados (si los hay)
        if (ib.movimientos_aprendidos != null && ib.movimientos_aprendidos.Count > 0)
        {
            var movesRestaurados = new List<Move>();
            foreach (var movAprendido in ib.movimientos_aprendidos)
            {
                string movNombre = catalogo.GetMovimientoNombre(movAprendido.numero);
                if (movNombre == null)
                {
                    Debug.LogWarning($"[IbermonConverter] Movimiento número {movAprendido.numero} no encontrado en catálogo.");
                    continue;
                }
                if (_moveBases.TryGetValue(movNombre, out var mb))
                {
                    var move = new Move(mb);
                    move.PP = movAprendido.pp;   // restaurar PP actuales guardados en la API
                    movesRestaurados.Add(move);
                }
                else
                    Debug.LogWarning($"[IbermonConverter] MoveBase '{movNombre}' no encontrado en Resources. " +
                                     "Revisa que el nombre del asset coincide con el catálogo.");
            }

            if (movesRestaurados.Count > 0)
                pokemon.Moves = movesRestaurados;
            // Si no se encontró ninguno, se conservan los que Init() asignó por nivel
        }

        return pokemon;
    }

    /// <summary>Convierte una lista de IbermonJugador en una lista de Pokemon.</summary>
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

    // ─── Unity → API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un IbermonJugadorActualizarRequest con el estado actual del Pokemon.
    /// Usado para sincronizar después de un combate.
    /// </summary>
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

    /// <summary>
    /// Obtiene el ibermon_catalogo_id de un Pokemon buscando su nombre en el catálogo.
    /// Devuelve -1 si no se encuentra (nombre del ScriptableObject no coincide con catálogo).
    /// </summary>
    public static int GetCatalogoId(Pokemon pokemon, CatalogoCache catalogo)
    {
        return catalogo.GetIbermonNumero(pokemon.Base.Name);
    }

    /// <summary>Invalida la caché de ScriptableObjects (útil en desarrollo si se añaden assets).</summary>
    public static void InvalidarCache()
    {
        _pokemonBases = null;
        _moveBases    = null;
    }
}
