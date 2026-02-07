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
            // Buscar el PokemonBase correspondiente desde AssetsPropios/Pokemons/Pokemons
            PokemonBase[] todosLosPokemon = Resources.LoadAll<PokemonBase>("AssetsPropios/Pokemons/Pokemons");
            
            // Si no funciona Resources, buscar manualmente en todo el proyecto
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
            
            PokemonBase pBase = null;
            foreach (var p in todosLosPokemon)
            {
                if (p != null && p.Name == pokemonGuardado.nombreBase)
                {
                    pBase = p;
                    break;
                }
            }
            
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
