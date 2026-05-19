using Assets.Scripts.Batalla;
using System.Collections;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DireccionEntrenador { Arriba, Abajo, Izquierda, Derecha }

public class TrainerController : MonoBehaviour
{
    private const string NombreEntrenadorPorDefecto = "Paloma";

    [Header("Campo de vision")]
    [SerializeField] GameObject campoDeVision;
    [SerializeField] DireccionEntrenador direccion = DireccionEntrenador.Abajo;
    [SerializeField] GameObject exclamacion;

    [Header("Movimiento")]
    [SerializeField] float velocidadMovimiento = 3f;
    [SerializeField] string nombreEntrenadorApi = "Paloma";

    PokemonParty trainerParty;
    Rigidbody2D rb;
    bool batallaYaIniciada = false;
    private bool yaDerotado = false;
    private string claveGuardado;

    void Awake()
    {
        trainerParty = GetComponent<PokemonParty>();
        rb = GetComponent<Rigidbody2D>();
        claveGuardado = $"Entrenador_{gameObject.name}_Derrotado";
        yaDerotado = PlayerPrefs.GetInt(claveGuardado, 0) == 1;
    }

    void Start()
    {
        AplicarRotacionFOV();
        if (exclamacion != null) exclamacion.SetActive(false);
    }

    private void AplicarRotacionFOV()
    {
        if (campoDeVision == null) return;
        float angulo = direccion switch
        {
            DireccionEntrenador.Arriba => 0f,
            DireccionEntrenador.Derecha => 90f,
            DireccionEntrenador.Abajo => 180f,
            DireccionEntrenador.Izquierda => 270f,
            _ => 0f
        };
        campoDeVision.transform.eulerAngles = new Vector3(0f, 0f, angulo);
    }

    public void JugadorDetectado(Transform jugador)
    {
        if (batallaYaIniciada) return;
        if (yaDerotado) return;
        batallaYaIniciada = true;

        StartCoroutine(SecuenciaEntrenador(jugador));
    }

    private IEnumerator MoverHaciaJugador(Transform jugador)
    {
        while (true)
        {
            Vector2 dirHaciaJugador = (jugador.position - transform.position);
            float distancia = dirHaciaJugador.magnitude;

            if (distancia <= 0.6f)
            {
                DetenerMovimiento();
                yield break;
            }

            rb.linearVelocity = dirHaciaJugador.normalized * velocidadMovimiento;
            yield return null;
        }
    }

    public void DetenerMovimiento()
    {
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private IEnumerator SecuenciaEntrenador(Transform jugador)
    {
        Movimiento movJugador = jugador.GetComponent<Movimiento>();
        if (movJugador != null) movJugador.estaEnInteraccion = true;

        if (exclamacion != null)
        {
            exclamacion.SetActive(true);
            yield return new WaitForSeconds(0.8f);
            exclamacion.SetActive(false);
        }

        yield return MoverHaciaJugador(jugador);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitForSeconds(0.3f);

        Interactuable interactuable = GetComponentInChildren<Interactuable>();
        Debug.Log($"[TrainerController] Interactuable encontrado: {interactuable}");

        if (interactuable != null)
            interactuable.IniciarDialogoDesdeEntrenador();
        else
            IniciarBatallaEntrenador(jugador);
    }

    public void IniciarBatallaEntrenador(Transform jugador)
    {
        JugadorSpawn.posicion = jugador.position;
        JugadorSpawn.escenaAnterior = SceneManager.GetActiveScene().name;
        BattleData.NombreEntrenador = string.IsNullOrWhiteSpace(nombreEntrenadorApi)
            ? NombreEntrenadorPorDefecto
            : nombreEntrenadorApi;

        if (ApiSetup.Entrenador != null && CatalogoCache.Instance != null && CatalogoCache.Instance.EstaListo)
        {
            ApiSetup.Entrenador.ObtenerPorNombre(BattleData.NombreEntrenador,
                detalle => LanzarBatallaConDatosApi(detalle),
                error => LanzarBatallaConFallbackLocal(error));
        }
        else
        {
            LanzarBatallaConFallbackLocal("API o CatalogoCache no disponibles");
        }
    }

    private void LanzarBatallaConDatosApi(EntrenadorCatalogoDetalle detalle)
    {
        List<Pokemon> equipo = IbermonConverter.ToPokemonsFromEntrenador(detalle.equipo, CatalogoCache.Instance);
        if (equipo == null || equipo.Count == 0)
        {
            LanzarBatallaConFallbackLocal("Equipo vacio tras convertir DTO de la API");
            return;
        }

        Debug.Log($"[TrainerController] Equipo de '{detalle.nombre}' cargado desde API ({equipo.Count} ibermon).");
        BattleData.TrainerPokemons = equipo;
        BattleData.EsEntrenador = true;
        BattleData.WildPokemon = null;
        BattleData.NombreEntrenador = detalle.nombre;
        SceneManager.LoadScene("Combate");
    }

    private void LanzarBatallaConFallbackLocal(string motivo)
    {
        Debug.LogWarning($"[TrainerController] Fallback al PokemonParty local. Motivo: {motivo}");

        if (trainerParty == null || trainerParty.Pokemons == null || trainerParty.Pokemons.Count == 0)
        {
            Debug.LogError($"[TrainerController] '{gameObject.name}' no tiene PokemonParty fallback.");
            return;
        }

        List<Pokemon> equipoFallback = CrearEquipoFallbackDesdePokemonPartyLocal();
        BattleData.TrainerPokemons = equipoFallback ?? new List<Pokemon>(trainerParty.Pokemons);
        BattleData.EsEntrenador = true;
        BattleData.WildPokemon = null;
        SceneManager.LoadScene("Combate");
    }

    private List<Pokemon> CrearEquipoFallbackDesdePokemonPartyLocal()
    {
        if (CatalogoCache.Instance == null || !CatalogoCache.Instance.EstaListo)
            return null;

        if (trainerParty.Pokemons == null || trainerParty.Pokemons.Count == 0)
            return null;

        CatalogoCache catalogo = CatalogoCache.Instance;
        List<IbermonJugador> equipoApi = new List<IbermonJugador>();

        foreach (Pokemon pokemonLocal in trainerParty.Pokemons)
        {
            if (pokemonLocal?.Base == null)
                return null;

            int ibermonCatalogoId = catalogo.GetIbermonNumero(pokemonLocal.Base.Name);
            if (ibermonCatalogoId <= 0)
            {
                Debug.LogWarning($"[TrainerController] '{pokemonLocal.Base.Name}' no esta en el catalogo API. Se usa equipo local.");
                return null;
            }

            Pokemon pokemonTemporal = new Pokemon(pokemonLocal.Base, pokemonLocal.Level);
            pokemonTemporal.Init();
            List<MovimientoAprendido> movimientos = new List<MovimientoAprendido>();

            if (pokemonTemporal.Moves != null)
            {
                foreach (Move move in pokemonTemporal.Moves)
                {
                    int numeroMovimiento = catalogo.GetMovimientoNumero(move.Base.Name);
                    if (numeroMovimiento > 0)
                        movimientos.Add(new MovimientoAprendido { numero = numeroMovimiento, pp = move.PP });
                }
            }

            equipoApi.Add(new IbermonJugador
            {
                ibermon_catalogo_id = ibermonCatalogoId,
                nivel = pokemonTemporal.Level,
                experiencia = pokemonTemporal.Exp,
                hp_actual = pokemonTemporal.MaxHp,
                ubicacion = "equipo",
                movimientos_aprendidos = movimientos,
            });
        }

        List<Pokemon> equipoConvertido = IbermonConverter.ToPokemons(equipoApi, catalogo);
        if (equipoConvertido.Count != equipoApi.Count)
            return null;

        return equipoConvertido;
    }
}
