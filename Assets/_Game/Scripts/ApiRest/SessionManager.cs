using System;
using System.Collections;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

// Guarda el estado de la sesión actual: la partida activa y el equipo.
// Se crea solo por ApiSetup, no añadir a ninguna escena manualmente.
public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public string PartidaId { get; private set; }
    public PartidaCompleta PartidaActual { get; private set; }
    public bool TienePartida => !string.IsNullOrEmpty(PartidaId);

    private List<IbermonJugador> _equipo = new();
    public IReadOnlyList<IbermonJugador> EquipoAPI => _equipo;

    public int CombatesGanados { get; set; }
    public int CombatesPerdidos { get; set; }

    private float _tiempoSesion   = 0f;
    private bool  _contandoTiempo = false;

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

    // Llámalo después de obtener la PartidaCompleta y el equipo de la API
    public void IniciarConPartida(PartidaCompleta partida, List<IbermonJugador> equipo)
    {
        if (partida == null)
        {
            Debug.LogError("[SessionManager] No se puede iniciar sesion con una partida null");
            return;
        }

        CompletarDatosPartida(partida);

        PartidaId = partida.id;
        PartidaActual = partida;
        _equipo = equipo ?? new List<IbermonJugador>();
        _tiempoSesion = partida.tiempo_jugado;
        CombatesGanados = partida.combates_ganados;
        CombatesPerdidos = partida.combates_perdidos;
        _contandoTiempo = true;
    }

    private void CompletarDatosPartida(PartidaCompleta partida)
    {
        if (string.IsNullOrWhiteSpace(partida.id))
            partida.id = PartidaId;

        if (PartidaActual != null && PartidaActual.id == partida.id)
        {
            if (string.IsNullOrWhiteSpace(partida.nombre))
                partida.nombre = PartidaActual.nombre;

            if (string.IsNullOrWhiteSpace(partida.personaje_elegido))
                partida.personaje_elegido = PartidaActual.personaje_elegido;

            if (string.IsNullOrWhiteSpace(partida.fecha_creacion))
                partida.fecha_creacion = PartidaActual.fecha_creacion;

            if (string.IsNullOrWhiteSpace(partida.ultima_conexion))
                partida.ultima_conexion = PartidaActual.ultima_conexion;
        }

        if (string.IsNullOrWhiteSpace(partida.nombre))
            partida.nombre = "Mi Partida";
    }

    public void ActualizarPartidaActual(PartidaCompleta partida)
    {
        if (partida == null)
        {
            Debug.LogWarning("[SessionManager] No se puede actualizar la partida con datos null");
            return;
        }

        CompletarDatosPartida(partida);
        PartidaId = partida.id;
        PartidaActual = partida;
        _tiempoSesion = partida.tiempo_jugado;
        CombatesGanados = partida.combates_ganados;
        CombatesPerdidos = partida.combates_perdidos;
    }

    // Llámalo al hacer logout o al volver al menú principal
    public void CerrarSesion()
    {
        PartidaId = null;
        PartidaActual = null;
        _equipo = new List<IbermonJugador>();
        _tiempoSesion = 0f;
        _contandoTiempo = false;
        CombatesGanados = 0;
        CombatesPerdidos = 0;
    }

    public IbermonJugador GetIbermonAPI(int index) =>
        index >= 0 && index < _equipo.Count ? _equipo[index] : null;

    public void SetEquipoAPI(List<IbermonJugador> equipo) =>
        _equipo = equipo ?? new List<IbermonJugador>();

    // Capturar un ibermon: lo añade localmente y lo registra en la API
    public void AnadirIbermon(
        int catalogoId,
        int nivel,
        int hpActual,
        int hpMaximo,
        List<int> movimientos,
        string ubicacion,
        Action<IbermonJugador> onSuccess,
        Action<string> onError)
    {
        if (!TienePartida) { onError?.Invoke("No hay partida activa"); return; }

        var req = new IbermonJugadorCrearRequest
        {
            ibermon_catalogo_id = catalogoId,
            nivel = nivel,
            hp_actual = hpActual,
            hp_maximo = hpMaximo,
            ubicacion = ubicacion,
        };

        ApiSetup.IbermonJugador.AnadirIbermon(PartidaId, req,
            ibermon =>
            {
                if (ubicacion == "equipo") _equipo.Add(ibermon);
                onSuccess?.Invoke(ibermon);
            },
            onError);
    }

    // Sincronizar el equipo completo con la API después de un combate
    public void SincronizarEquipo(
        List<Pokemon> pokemons,
        CatalogoCache catalogo,
        Action onDone,
        Action<string> onError)
    {
        if (!TienePartida) { onError?.Invoke("No hay partida activa"); return; }
        StartCoroutine(SincronizarEquipoCoroutine(pokemons, catalogo, onDone, onError));
    }

    private IEnumerator SincronizarEquipoCoroutine(
        List<Pokemon> pokemons,
        CatalogoCache catalogo,
        Action onDone,
        Action<string> onError)
    {
        bool hubieronErrores = false;

        for (int i = 0; i < pokemons.Count; i++)
        {
            var ibermonApi = GetIbermonAPI(i);
            if (ibermonApi == null) continue;

            var request = IbermonConverter.ToActualizarRequest(pokemons[i], catalogo);

            bool terminado    = false;
            string errorParcial = null;

            ApiSetup.IbermonJugador.ActualizarIbermon(
                PartidaId, ibermonApi.id, request,
                _  => terminado = true,
                err => { errorParcial = err; terminado = true; }
            );

            yield return new WaitUntil(() => terminado);

            if (errorParcial != null)
            {
                Debug.LogWarning($"[SessionManager] Error sync ibermon[{i}]: {errorParcial}");
                hubieronErrores = true;
            }
        }

        if (hubieronErrores) onError?.Invoke("Algunos ibermon no se pudieron sincronizar");
        else onDone?.Invoke();
    }

    // Guardar mapa, posición, dinero y tiempo — llámalo en el centro ibermon o al salir
    public void GuardarPartida(
        string mapaActual,
        Vector2 posicion,
        int dinero,
        Action onDone,
        Action<string> onError)
    {
        if (!TienePartida) { onError?.Invoke("No hay partida activa"); return; }

        var datos = new GuardarPartidaRequest
        {
            mapa_actual = mapaActual,
            posicion = new Posicion { x = posicion.x, y = posicion.y },
            dinero = dinero,
            tiempo_jugado = Mathf.RoundToInt(_tiempoSesion),
            combates_ganados = CombatesGanados,
            combates_perdidos = CombatesPerdidos,
        };

        // Conservar pokedex, medallas, logros y flags de la partida anterior
        if (PartidaActual != null)
        {
            datos.pokedex_visto = PartidaActual.pokedex_visto;
            datos.pokedex_capturado = PartidaActual.pokedex_capturado;
            datos.medallas = PartidaActual.medallas;
            datos.logros = PartidaActual.logros;
            datos.flags = PartidaActual.flags;
        }

        ApiSetup.Partida.GuardarPartida(PartidaId, datos,
            partida => { ActualizarPartidaActual(partida); onDone?.Invoke(); },
            onError);
    }

    public int TiempoJugadoSegundos => Mathf.RoundToInt(_tiempoSesion);
}
