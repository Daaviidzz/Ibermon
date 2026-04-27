using System.Collections.Generic;
using System.Linq;
using ApiRest.Models;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;
    [SerializeField] bool esEquipoJugador = false;

    private bool esBatallaTemp = false;

    public List<Pokemon> Pokemons => pokemons;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        if (esBatallaTemp) return;
        if (esEquipoJugador)
            CargarEquipoGuardado();
    }

    // ─── Carga de equipo ──────────────────────────────────────────────────────

    /// <summary>
    /// Carga el equipo desde la sesión activa de la API.
    /// Prioridad 1: SessionManager (API) — usa IbermonConverter.
    /// Prioridad 2: PlayerPrefs (fallback legacy, solo si no hay sesión API).
    /// </summary>
    public void CargarEquipoGuardado()
    {
        // ── Prioridad 1: datos de la API (sesión activa) ─────────────────────
        if (SessionManager.Instance != null &&
            SessionManager.Instance.TienePartida &&
            CatalogoCache.Instance != null &&
            CatalogoCache.Instance.EstaListo)
        {
            var equipoApi = new List<IbermonJugador>(SessionManager.Instance.EquipoAPI);
            if (equipoApi.Count > 0)
            {
                pokemons = IbermonConverter.ToPokemons(equipoApi, CatalogoCache.Instance);
                if (pokemons.Count > 0)
                {
                    Debug.Log($"[PokemonParty] Equipo cargado desde API: {pokemons.Count} ibermon.");
                    return;
                }
                Debug.LogWarning("[PokemonParty] La API devolvió equipo pero no se pudieron convertir. " +
                                 "¿Los nombres de los ScriptableObjects coinciden con el catálogo?");
            }
            else
            {
                Debug.Log("[PokemonParty] La partida no tiene ibermon en el equipo todavía.");
                pokemons = new List<Pokemon>();
                return;
            }
        }

        // ── Prioridad 2: PlayerPrefs (legacy / sin conexión) ─────────────────
        if (SistemGuardadoPokemon.HayDatosGuardados())
        {
            var cargado = SistemGuardadoPokemon.CargarEquipo();
            if (cargado != null && cargado.Count > 0)
            {
                pokemons = cargado;
                Debug.Log("[PokemonParty] Equipo cargado desde PlayerPrefs (modo sin API).");
                return;
            }
        }

        Debug.LogWarning("[PokemonParty] No se encontró equipo. El partido puede no haberse cargado aún.");
    }

    // ─── Batalla de entrenador ────────────────────────────────────────────────

    public void SetPokemonsForBattle(List<Pokemon> pokemonsEntrenador)
    {
        esBatallaTemp = true;
        pokemons = pokemonsEntrenador;
        foreach (var p in pokemons) p.Init();
    }

    // ─── Helpers de combate ───────────────────────────────────────────────────

    /// <summary>Devuelve el primer ibermon con HP > 0.</summary>
    public Pokemon GetHealtyPokemon() =>
        pokemons?.FirstOrDefault(p => p.HP > 0);

    /// <summary>
    /// Añade un nuevo ibermon al equipo local (al capturar).
    /// La persistencia en la API se gestiona desde BattleSystem mediante SessionManager.
    /// </summary>
    public bool AddPokemon(Pokemon newPokemon)
    {
        if (pokemons == null) pokemons = new List<Pokemon>();

        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            return true;
        }
        return false;
    }

    /// <summary>Cura a todos los ibermon del equipo (HP máximo, sin status).</summary>
    public void HealAllPokemonsInParty()
    {
        if (pokemons == null) return;
        foreach (var p in pokemons)
        {
            p.ResetHealth();
            p.CureStatus();
        }
        // La sincronización con la API se hace explícitamente (SessionManager.SincronizarEquipo)
        // no aquí, para evitar llamadas duplicadas.
    }
}
