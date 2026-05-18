using Assets.Scripts.Batalla;
using System.Collections;
using System.Collections.Generic;
using ApiRest.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DireccionEntrenador { Arriba, Abajo, Izquierda, Derecha }

public class TrainerController : MonoBehaviour
{
    [Header("Campo de visión")]
    [SerializeField]  GameObject campoDeVision;
    [SerializeField]  DireccionEntrenador direccion = DireccionEntrenador.Abajo;
    [SerializeField]  GameObject exclamacion; 

    [Header("Movimiento")]
    [SerializeField]  float velocidadMovimiento = 3f;

     PokemonParty trainerParty;
     Rigidbody2D rb;
     bool batallaYaIniciada = false; // Para que solo se dispare una vez
    private bool yaDerotado = false;
    private string claveGuardado; // clave única por entrenador

    void Awake()
    {
        trainerParty = GetComponent<PokemonParty>();
        rb = GetComponent<Rigidbody2D>();
        // Usamos el nombre del GameObject como clave única
        claveGuardado = $"Entrenador_{gameObject.name}_Derrotado";
        yaDerotado = PlayerPrefs.GetInt(claveGuardado, 0) == 1;
    }

     void Start()
    {
        AplicarRotacionFOV();
        if (exclamacion != null) exclamacion.SetActive(false);
    }

    // Rota el hijo CampoDeVision según la dirección elegida en el Inspector
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

    // El collider del hijo llama a esto cuando detecta al jugador
    public void JugadorDetectado(Transform jugador)
    {
        if (batallaYaIniciada) return;
        if (yaDerotado) return; // Ya fue derrotado, ignorar
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
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // Congela la posición
    }
   
    private IEnumerator SecuenciaEntrenador(Transform jugador)
    {
        var movJugador = jugador.GetComponent<Movimiento>();
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

        
        var interactuable = GetComponentInChildren<Interactuable>();
        Debug.Log($"Interactuable encontrado: {interactuable}");

        if (interactuable != null)
            interactuable.IniciarDialogoDesdeEntrenador();
        else
            IniciarBatallaEntrenador(jugador);
    }

    public void IniciarBatallaEntrenador(Transform jugador)
    {
        if (trainerParty == null)
        {
            Debug.LogError($"[{gameObject.name}] No tiene PokemonParty asignada");
            return;
        }

        JugadorSpawn.posicion = jugador.position;
        JugadorSpawn.escenaAnterior = SceneManager.GetActiveScene().name;
        BattleData.TrainerPokemons = CrearEquipoEntrenadorDesdeApi() ?? new List<Pokemon>(trainerParty.Pokemons);
        BattleData.EsEntrenador = true;
        BattleData.WildPokemon = null;
        BattleData.NombreEntrenador = gameObject.name; // ← AÑADE ESTO
        SceneManager.LoadScene("Combate");
    }

    private List<Pokemon> CrearEquipoEntrenadorDesdeApi()
    {
        if (CatalogoCache.Instance == null || !CatalogoCache.Instance.EstaListo)
            return null;

        if (trainerParty.Pokemons == null || trainerParty.Pokemons.Count == 0)
            return null;

        var catalogo = CatalogoCache.Instance;
        var equipoApi = new List<IbermonJugador>();

        foreach (var pokemonLocal in trainerParty.Pokemons)
        {
            if (pokemonLocal?.Base == null)
                return null;

            int ibermonCatalogoId = catalogo.GetIbermonNumero(pokemonLocal.Base.Name);
            if (ibermonCatalogoId <= 0)
            {
                Debug.LogWarning($"[TrainerController] '{pokemonLocal.Base.Name}' no está en el catálogo API. Se usa equipo local.");
                return null;
            }

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

        var equipoConvertido = IbermonConverter.ToPokemons(equipoApi, catalogo);
        if (equipoConvertido.Count != equipoApi.Count)
            return null;

        Debug.Log($"[TrainerController] Equipo de entrenador cargado desde API: {equipoConvertido.Count} ibermon.");
        return equipoConvertido;
    }
}
