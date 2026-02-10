using System.Collections.Generic;
using UnityEngine;

// Clase simplificada para guardar solo los datos esenciales de un Pokémon
[System.Serializable]
public class PokemonGuardado
{
    public string nombreBase; // Nombre del PokemonBase
    public int nivel;
    public int hp;
    public int exp;

    public PokemonGuardado(Pokemon pokemon)
    {
        nombreBase = pokemon.Base.Name;
        nivel = pokemon.Level;
        hp = pokemon.HP;
        exp = pokemon.Exp;
    }
}

// Contenedor para la lista completa de Pokémon
[System.Serializable]
public class DatosEquipoPokemon
{
    public List<PokemonGuardado> equipo;
}

public static class SistemGuardadoPokemon
{
    private const string CLAVE_GUARDADO = "EquipoPokemon";

    // Nombre del Pokémon inicial por defecto
    private const string POKEMON_INICIAL = "Charmander";
    private const int NIVEL_INICIAL = 9;

    // ========== NUEVA PARTIDA ==========
    // Borra datos guardados y crea equipo con Pokémon inicial
    public static List<Pokemon> NuevaPartida()
    {
        BorrarDatos();

        List<Pokemon> equipoNuevo = new List<Pokemon>();

        // Crear Pokémon inicial
        PokemonBase pokemonBase = ObtenerPokemonBasePorNombre(POKEMON_INICIAL);

        if (pokemonBase != null)
        {
            Pokemon pokemonInicial = new Pokemon(pokemonBase, NIVEL_INICIAL);
            pokemonInicial.Init();
            equipoNuevo.Add(pokemonInicial);

            Debug.Log($"Nueva partida iniciada con {POKEMON_INICIAL} nivel {NIVEL_INICIAL}");
        }
        else
        {
            Debug.LogError($"No se encontró el Pokémon inicial: {POKEMON_INICIAL}");
        }

        // Guardar equipo inicial
        GuardarEquipo(equipoNuevo);

        return equipoNuevo;
    }

    // ========== CARGAR PARTIDA ==========
    // Carga partida guardada SIN borrar datos
    public static List<Pokemon> CargarPartida()
    {
        if (!HayDatosGuardados())
        {
            Debug.LogWarning("No hay partida guardada. Creando nueva partida...");
            return NuevaPartida();
        }

        List<Pokemon> equipoCargado = CargarEquipo();

        if (equipoCargado == null || equipoCargado.Count == 0)
        {
            Debug.LogWarning("Error al cargar equipo. Creando nueva partida...");
            return NuevaPartida();
        }

        Debug.Log($"Partida cargada exitosamente con {equipoCargado.Count} Pokémon");
        return equipoCargado;
    }


    // Buscar PokemonBase por nombre
    private static PokemonBase ObtenerPokemonBasePorNombre(string nombre)
    {
        // Buscar en Resources
        PokemonBase[] todosLosPokemon = Resources.LoadAll<PokemonBase>("AssetsPropios/Pokemons/Pokemons");

        // Si Resources no funciona, buscar en todo el proyecto (solo en Editor)
        if (todosLosPokemon == null || todosLosPokemon.Length == 0)
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:PokemonBase");
            todosLosPokemon = new PokemonBase[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                todosLosPokemon[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<PokemonBase>(path);
            }
#endif
        }

        foreach (var pokemon in todosLosPokemon)
        {
            if (pokemon != null && pokemon.Name == nombre)
            {
                return pokemon;
            }
        }

        return null;
    }

    // Guardar el equipo completo en disco
    public static void GuardarEquipo(List<Pokemon> pokemons)
    {
        DatosEquipoPokemon datos = new DatosEquipoPokemon();
        datos.equipo = new List<PokemonGuardado>();

        foreach (var pokemon in pokemons)
        {
            datos.equipo.Add(new PokemonGuardado(pokemon));
        }

        string json = JsonUtility.ToJson(datos);
        PlayerPrefs.SetString(CLAVE_GUARDADO, json);
        PlayerPrefs.Save();

        Debug.Log($"Equipo guardado: {pokemons.Count} Pokémon");
    }

    // Cargar el equipo desde disco
    public static List<Pokemon> CargarEquipo()
    {
        if (!PlayerPrefs.HasKey(CLAVE_GUARDADO))
        {
            Debug.Log("No hay datos guardados");
            return null;
        }

        string json = PlayerPrefs.GetString(CLAVE_GUARDADO);
        DatosEquipoPokemon datos = JsonUtility.FromJson<DatosEquipoPokemon>(json);

        List<Pokemon> equipoCargado = new List<Pokemon>();

        foreach (var pokemonGuardado in datos.equipo)
        {
            PokemonBase pBase = ObtenerPokemonBasePorNombre(pokemonGuardado.nombreBase);

            if (pBase != null)
            {
                Pokemon pokemon = new Pokemon(pBase, pokemonGuardado.nivel);
                pokemon.Init();
                pokemon.HP = pokemonGuardado.hp;
                pokemon.Exp = pokemonGuardado.exp;
                equipoCargado.Add(pokemon);
            }
            else
            {
                Debug.LogWarning($"No se encontró PokemonBase: {pokemonGuardado.nombreBase}");
            }
        }

        Debug.Log($"Equipo cargado: {equipoCargado.Count} Pokémon");
        return equipoCargado;
    }

    // Verificar si hay datos guardados
    public static bool HayDatosGuardados()
    {
        return PlayerPrefs.HasKey(CLAVE_GUARDADO);
    }

    // Borrar todos los datos guardados
    public static void BorrarDatos()
    {
        PlayerPrefs.DeleteKey(CLAVE_GUARDADO);
        PlayerPrefs.Save();
        Debug.Log("Datos de Pokémon borrados");
    }
}