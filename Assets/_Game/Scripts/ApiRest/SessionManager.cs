using System;
using System.Collections;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

/// <summary>
/// Singleton DontDestroyOnLoad.
/// Almacena el estado de la sesión de juego actual:
/// la partida activa, el equipo (modelos API) y estadísticas de sesión.
/// Se crea automáticamente por ApiSetup. No añadir manualmente a la escena.
/// </summary>
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    // ─── Datos de partida ─────────────────────────────────────────────────────

    public string PartidaId { get; private set; }
    public PartidaCompleta PartidaActual { get; private set; }
    public bool TienePartida => !string.IsNullOrEmpty(PartidaId);

    // ─── Equipo (modelos API) ─────────────────────────────────────────────────

    private List<IbermonJugador> _equipo = new();
    public IReadOnlyList<IbermonJugador> EquipoAPI => _equipo;

    // ─── Estadísticas de sesión ───────────────────────────────────────────────

    public int CombatesGanados  { get; set; }
    public int CombatesPerdidos { get; set; }

    private float _tiempoSesion   = 0f;
    private bool  _contandoTiempo = false;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (_contandoTiempo)
            _tiempoSesion += Time.deltaTime;
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    /// <summary>
    /// Llama esto después de obtener la PartidaCompleta y el equipo de la API.
    /// Inicializa toda la sesión de juego.
    /// </summary>
    public void IniciarConPartida(PartidaCompleta partida, List<IbermonJugador> equipo)
    {
        PartidaId       = partida.id;
        PartidaActual   = partida;
        _equipo         = equipo ?? new List<IbermonJugador>();
        _tiempoSesion   = partida.tiempo_jugado;
        CombatesGanados  = partida.combates_ganados;
        CombatesPerdidos = partida.combates_perdidos;
        _contandoTiempo = true;
    }

    /// <summary>Limpia la sesión al hacer logout o volver al menú.</summary>
    public void CerrarSesion()
    {
        PartidaId        = null;
        PartidaActual    = null;
        _equipo          = new List<IbermonJugador>();
        _tiempoSesion    = 0f;
        _contandoTiempo  = false;
        CombatesGanados  = 0;
        CombatesPerdidos = 0;
    }

    /// <summary>Devuelve el IbermonJugador (API) en la posición dada del equipo.</summary>
    public IbermonJugador GetIbermonAPI(int index) =>
        index >= 0 && index < _equipo.Count ? _equipo[index] : null;

    /// <summary>Reemplaza la lista del equipo (ej. tras recargar de la API).</summary>
    public void SetEquipoAPI(List<IbermonJugador> equipo) =>
        _equipo = equipo ?? new List<IbermonJugador>();

    /// <summary>Añade un ibermon al equipo local (al capturar) y lo registra en la API.</summary>
    public void AnadirIbermon(
        int    catalogoId,
        int    nivel,
        int    hpActual,
        List<int> movimientos,
        string ubicacion,
        Action<IbermonJugador> onSuccess,
        Action<string>         onError)
    {
        if (!TienePartida) { onError?.Invoke("No hay partida activa"); return; }

        var req = new IbermonJugadorCrearRequest
        {
            ibermon_catalogo_id = catalogoId,
            nivel               = nivel,
            hp_actual           = hpActual,
            ubicacion           = ubicacion,
        };

        ApiSetup.IbermonJugador.AnadirIbermon(PartidaId, req,
            ibermon =>
            {
                if (ubicacion == "equipo") _equipo.Add(ibermon);
                onSuccess?.Invoke(ibermon);
            },
            onError);
    }

    /// <summary>
    /// Sincroniza el estado actual del equipo de Unity (Pokemon[]) hacia la API.
    /// Actualiza HP, nivel, experiencia y movimientos de cada ibermon.
    /// </summary>
    public void SincronizarEquipo(
        List<Pokemon>  pokemons,
        CatalogoCache  catalogo,
        Action         onDone,
        Action<string> onError)
    {
        if (!TienePartida) { onError?.Invoke("No hay partida activa"); return; }
        StartCoroutine(SincronizarEquipoCoroutine(pokemons, catalogo, onDone, onError));
    }

    private IEnumerator SincronizarEquipoCoroutine(
        List<Pokemon>  pokemons,
        CatalogoCache  catalogo,
        Action         onDone,
        Action<string> onError)
    {
        bool hubieronErrores = false;

        for (int i = 0; i < pokemons.Count; i++)
        {
            var ibermonApi = GetIbermonAPI(i);
            if (ibermonApi == null) continue;

            var request = IbermonConverter.ToActualizarRequest(pokemons[i], catalogo);

            bool terminado = false;
            string errorParcial = null;

            ApiSetup.IbermonJugador.ActualizarIbermon(
                PartidaId, ibermonApi.id, request,
                _    => terminado = true,
                err  => { errorParcial = err; terminado = true; }
            );

            yield return new WaitUntil(() => terminado);

            if (errorParcial != null)
            {
                Debug.LogWarning($"[SessionManager] Error sync ibermon[{i}]: {errorParcial}");
                hubieronErrores = true;
            }
        }

        if (hubieronErrores) onError?.Invoke("Algunos ibermon no se pudieron sincronizar");
        else                 onDone?.Invoke();
    }

    /// <summary>
    /// Guarda el estado completo de la partida (mapa, posición, dinero, tiempo, stats).
    /// Llama esto al guardar en Centro Ibermon.
    /// </summary>
    public void GuardarPartida(
        string  mapaActual,
        Vector2 posicion,
        int     dinero,
        Action         onDone,
        Action<string> onError)
    {
        if (!TienePartida) { onError?.Invoke("No hay partida activa"); return; }

        var datos = new GuardarPartidaRequest
        {
            mapa_actual       = mapaActual,
            posicion          = new Posicion { x = posicion.x, y = posicion.y },
            dinero            = dinero,
            tiempo_jugado     = Mathf.RoundToInt(_tiempoSesion),
            combates_ganados  = CombatesGanados,
            combates_perdidos = CombatesPerdidos,
        };

        // Conservar datos de la partida anterior si existen
        if (PartidaActual != null)
        {
            datos.pokedex_visto     = PartidaActual.pokedex_visto;
            datos.pokedex_capturado = PartidaActual.pokedex_capturado;
            datos.medallas          = PartidaActual.medallas;
            datos.logros            = PartidaActual.logros;
            datos.flags             = PartidaActual.flags;
        }

        ApiSetup.Partida.GuardarPartida(PartidaId, datos,
            partida => { PartidaActual = partida; onDone?.Invoke(); },
            onError);
    }

    /// <summary>Devuelve el tiempo total jugado en esta sesión (segundos).</summary>
    public int TiempoJugadoSegundos => Mathf.RoundToInt(_tiempoSesion);
}
