using System.Collections.Generic;
using ApiRest.Models;
using UnityEngine;

// Convierte entre los modelos de la API (IbermonJugador) y los objetos del juego (Pokemon).
//
// Importante: los ScriptableObjects de PokemonBase tienen que estar en Resources/Pokemons/
// y sus nombres deben coincidir exactamente con el campo "nombre" del catálogo de la API.
// Lo mismo para MoveBase en Resources/Moves/. Si no coinciden, los ibermon no cargan.
public static class IbermonConverter
{
    private static Dictionary<string, PokemonBase> _pokemonBases;
    private static Dictionary<string, MoveBase>    _moveBases;

    // Carga todos los ScriptableObjects desde Resources la primera vez que se necesitan
    private static void EnsureResourcesLoaded()
    {
        if (_pokemonBases != null) return;

        _pokemonBases = new Dictionary<string, PokemonBase>();
        _moveBases    = new Dictionary<string, MoveBase>();

        foreach (var pb in Resources.LoadAll<PokemonBase>("Pokemons"))
        {
            if (pb == null) continue;
            _pokemonBases[pb.Name] = pb;

            // Aprovechar para indexar también los movimientos que tiene aprendibles
            if (pb.LearnableMoves == null) continue;
            foreach (var lm in pb.LearnableMoves)
            {
                if (lm?.MoveBase != null && !_moveBases.ContainsKey(lm.MoveBase.Name))
                    _moveBases[lm.MoveBase.Name] = lm.MoveBase;
            }
        }

        // También cargar movimientos que sean assets sueltos en Resources/Moves/
        foreach (var mb in Resources.LoadAll<MoveBase>("Moves"))
        {
            if (mb != null && !_moveBases.ContainsKey(mb.Name))
                _moveBases[mb.Name] = mb;
        }

        Debug.Log($"[IbermonConverter] PokemonBase: {_pokemonBases.Count} | MoveBase: {_moveBases.Count}");
    }

    // API : Unity: convierte un IbermonJugador en un Pokemon listo para combatir
    public static Pokemon ToPokemon(IbermonJugador ib, CatalogoCache catalogo)
    {
        EnsureResourcesLoaded();

        string nombre = catalogo.GetIbermonNombre(ib.ibermon_catalogo_id);
        if (string.IsNullOrEmpty(nombre))
        {
            Debug.LogError($"[IbermonConverter] No se encontró el ibermon con id={ib.ibermon_catalogo_id} en el catálogo. " +
                           "¿Se cargaron los catálogos? ¿Tiene datos el servidor?");
            return null;
        }

        if (!_pokemonBases.TryGetValue(nombre, out var pBase))
        {
            Debug.LogError($"[IbermonConverter] No existe el PokemonBase '{nombre}' en Resources/Pokemons/. " +
                           "El nombre del asset debe coincidir exactamente con el del catálogo.");
            return null;
        }

        // Init() calcula stats, HP máximo y movimientos según el nivel
        var pokemon = new Pokemon(pBase, ib.nivel);
        pokemon.Init();

        // Sobreescribir con los valores reales guardados en la API
        pokemon.HP  = Mathf.Clamp(ib.hp_actual, 0, pokemon.MaxHp);
        pokemon.Exp = ib.experiencia;

        // Sprites desde la API (paths como "1.png" / "back/1.png")
        pokemon.FrontSprite = CargarSpriteDesdePath(catalogo.GetSpriteFrontal(ib.ibermon_catalogo_id));
        pokemon.BackSprite  = CargarSpriteDesdePath(catalogo.GetSpriteTrasero(ib.ibermon_catalogo_id));

        // Restaurar los movimientos guardados si los hay
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
                    Debug.LogWarning($"[IbermonConverter] No existe el MoveBase '{movNombre}' en Resources. " +
                                     "Comprueba que el nombre del asset coincide con el catálogo.");
            }

            if (movesRestaurados.Count > 0)
                pokemon.Moves = movesRestaurados;
            // Si no se restauró ninguno, se quedan los que asignó Init() por nivel
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

    public static List<Pokemon> ToPokemonsFromEntrenador(
        List<EquipoEntrenadorEntrada> equipoApi, CatalogoCache catalogo)
    {
        EnsureResourcesLoaded();

        List<Pokemon> result = new List<Pokemon>();
        if (equipoApi == null) return result;

        foreach (EquipoEntrenadorEntrada entrada in equipoApi)
        {
            string nombreIbermon = catalogo.GetIbermonNombre(entrada.numero);
            if (string.IsNullOrEmpty(nombreIbermon))
            {
                Debug.LogWarning($"[IbermonConverter] Ibermon #{entrada.numero} del entrenador no esta en el catalogo.");
                continue;
            }

            if (!_pokemonBases.TryGetValue(nombreIbermon, out PokemonBase pBase))
            {
                Debug.LogWarning($"[IbermonConverter] No existe el PokemonBase '{nombreIbermon}' en Resources.");
                continue;
            }

            Pokemon pokemon = new Pokemon(pBase, entrada.nivel);
            pokemon.Init();

            pokemon.FrontSprite = CargarSpriteDesdePath(catalogo.GetSpriteFrontal(entrada.numero));
            pokemon.BackSprite = CargarSpriteDesdePath(catalogo.GetSpriteTrasero(entrada.numero));

            if (entrada.movs != null && entrada.movs.Count > 0)
            {
                List<Move> movesPersonalizados = new List<Move>();
                foreach (int numeroMov in entrada.movs)
                {
                    string nombreMov = catalogo.GetMovimientoNombre(numeroMov);
                    if (nombreMov == null) continue;

                    if (_moveBases.TryGetValue(nombreMov, out MoveBase mb))
                        movesPersonalizados.Add(new Move(mb));
                }

                if (movesPersonalizados.Count > 0)
                    pokemon.Moves = movesPersonalizados;
            }

            result.Add(pokemon);
        }

        return result;
    }

    // Unity : API: prepara el request para sincronizar un pokemon después de un combate
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

    // Carga un Sprite desde Resources/Sprites/Pokemon/ usando el path de la API.
    // Acepta rutas relativas ("Kotlin/Kotlin.png") y rutas servidas desde /sprites/.
    private static Sprite CargarSpriteDesdePath(string apiPath)
    {
        string sinExt = NormalizarPathSprite(apiPath);
        if (string.IsNullOrEmpty(sinExt))
            return null;

        string resourcesPath = $"Sprites/Pokemon/{sinExt}";

        Sprite sprite = Resources.Load<Sprite>(resourcesPath);
        if (sprite != null)
            return sprite;

        var sprites = Resources.LoadAll<Sprite>(resourcesPath);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[IbermonConverter] No se encontró sprite en Resources/{resourcesPath} (api='{apiPath}')");
            return null;
        }
        return sprites[0];
    }

    private static string NormalizarPathSprite(string apiPath)
    {
        if (string.IsNullOrWhiteSpace(apiPath))
            return null;

        string path = apiPath.Trim().Replace('\\', '/');
        int queryIndex = path.IndexOf('?');
        if (queryIndex >= 0)
            path = path[..queryIndex];

        const string spritesSegment = "/sprites/";
        int spritesIndex = path.IndexOf(spritesSegment, System.StringComparison.OrdinalIgnoreCase);
        if (spritesIndex >= 0)
            path = path[(spritesIndex + spritesSegment.Length)..];
        else if (path.StartsWith("sprites/", System.StringComparison.OrdinalIgnoreCase))
            path = path["sprites/".Length..];

        path = path.TrimStart('/');
        if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            path = path[..^4];

        return path;
    }
}
