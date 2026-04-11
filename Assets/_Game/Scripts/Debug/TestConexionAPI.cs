using System.Collections;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

/// <summary>
/// Script de prueba para verificar la conexión Unity ↔ API sin necesitar gráficos.
///
/// INSTRUCCIONES:
///   1. Crea una escena vacía (File > New Scene > Empty)
///   2. Crea un GameObject vacío y añádele los dos componentes:
///        - ApiSetup  (configura la baseUrl al inspector, ej: http://localhost:8000)
///        - TestConexionAPI  (rellena Usuario, Contraseña en el Inspector)
///   3. Pulsa Play y mira la consola.
///
/// Cada paso loguea ✅ si va bien o ❌ si falla, con el detalle del error.
/// </summary>
[RequireComponent(typeof(ApiSetup))]
public class TestConexionAPI : MonoBehaviour
{
    [Header("Credenciales de prueba")]
    [SerializeField] private string usuario    = "test";
    [SerializeField] private string contrasena = "test1234";

    [Header("Opciones")]
    [Tooltip("Si está marcado, al final del test sincroniza el primer ibermon del equipo de vuelta a la API.")]
    [SerializeField] private bool testSincronizarIbermon = false;

    [Tooltip("Si no hay partidas, crea una automáticamente con estos valores.")]
    [SerializeField] private bool crearPartidaSiNoExiste = true;
    [SerializeField] private string personajeElegido = "Protagonista";
    [SerializeField] private int starterElegido = 1;

    // -----------------------------------------------------------------------

    private void Start()
    {
        StartCoroutine(EjecutarTests());
    }

    private IEnumerator EjecutarTests()
    {
        Log("══════════════════════════════════════════");
        Log("   TEST CONEXIÓN UNITY ↔ API IBERMON");
        Log("══════════════════════════════════════════");

        // Esperar a que ApiSetup termine su Awake
        yield return null;

        // ── 1. Ping ─────────────────────────────────────────────────────────
        yield return StartCoroutine(TestPing());

        // ── 2. Login ────────────────────────────────────────────────────────
        bool loginOk = false;
        yield return StartCoroutine(TestLogin(result => loginOk = result));
        if (!loginOk) { LogError("Login fallido — abortando tests."); yield break; }

        // ── 3. Catálogos ────────────────────────────────────────────────────
        yield return StartCoroutine(TestCatalogos());

        // ── 4. Listar partidas ──────────────────────────────────────────────
        string partidaId = null;
        yield return StartCoroutine(TestListarPartidas(id => partidaId = id));

        if (string.IsNullOrEmpty(partidaId))
        {
            if (crearPartidaSiNoExiste)
                yield return StartCoroutine(TestCrearPartida(id => partidaId = id));
            else
                LogWarning("No hay partidas disponibles — se omiten los tests de equipo.");
        }

        if (!string.IsNullOrEmpty(partidaId))
        {
            // ── 5. Obtener equipo ────────────────────────────────────────────
            List<IbermonJugador> equipo = null;
            yield return StartCoroutine(TestObtenerEquipo(partidaId, list => equipo = list));

            // ── 6. Converter ─────────────────────────────────────────────────
            if (equipo != null && equipo.Count > 0)
            {
                TestConverter(equipo);

                // ── 7. (Opcional) Sincronizar primer ibermon ──────────────────
                if (testSincronizarIbermon)
                    yield return StartCoroutine(TestSincronizarIbermon(partidaId, equipo[0]));
            }
        }

        Log("══════════════════════════════════════════");
        Log("   TESTS COMPLETADOS");
        Log("══════════════════════════════════════════");
    }

    // ── 1. Ping ──────────────────────────────────────────────────────────────

    private IEnumerator TestPing()
    {
        Log("[1/7] Ping al servidor...");
        bool done = false;

        ApiManager.Instance.Get("/",
            raw =>
            {
                LogOk($"Servidor alcanzable. Respuesta: {raw.Length} chars");
                done = true;
            },
            err =>
            {
                LogError($"No se puede conectar al servidor: {err}");
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── 2. Login ─────────────────────────────────────────────────────────────

    private IEnumerator TestLogin(System.Action<bool> result)
    {
        Log($"[2/7] Login como '{usuario}'...");
        bool done = false;

        ApiSetup.Auth.Login(usuario, contrasena,
            token =>
            {
                LogOk($"Login correcto. Token: {token.access_token.Substring(0, Mathf.Min(20, token.access_token.Length))}...");
                result(true);
                done = true;
            },
            err =>
            {
                LogError($"Login fallido: {err}");
                result(false);
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── 3. Catálogos ─────────────────────────────────────────────────────────

    private IEnumerator TestCatalogos()
    {
        Log("[3/7] Cargando catálogos (ibermon + movimientos)...");
        bool done = false;

        CatalogoCache.Instance.CargarCatalogos(
            () =>
            {
                // El cache ya tiene los datos, pero vamos a pedir un detalle para validar
                // el nuevo formato de movimientos_posibles
                ApiSetup.Catalogo.ObtenerIbermon(1,
                    detalle =>
                    {
                        LogOk($"Catálogos cargados. Ibermon #1: '{detalle.nombre}' " +
                              $"| catch_rate={detalle.catch_rate} exp_yield={detalle.exp_yield} growth_rate={detalle.growth_rate}");

                        if (detalle.movimientos_posibles.Count > 0)
                        {
                            var m = detalle.movimientos_posibles[0];
                            LogOk($"  Primer movimiento posible: numero={m.numero} nivel={m.nivel}");
                        }
                        else
                            LogWarning("  El ibermon #1 no tiene movimientos_posibles en la BD.");

                        // Verificar catálogo de movimientos
                        ApiSetup.Catalogo.ObtenerMovimiento(1,
                            mov =>
                            {
                                LogOk($"Movimiento #1: '{mov.nombre}' " +
                                      $"| categoria={mov.categoria} objetivo={mov.objetivo} " +
                                      $"prioridad={mov.prioridad} siempre_acierta={mov.siempre_acierta}");
                                done = true;
                            },
                            err => { LogWarning($"Movimiento #1 no encontrado: {err}"); done = true; });
                    },
                    err => { LogWarning($"Detalle ibermon #1 no disponible: {err}"); done = true; });
            },
            err =>
            {
                LogError($"Error cargando catálogos: {err}");
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── 4. Listar partidas ────────────────────────────────────────────────────

    private IEnumerator TestListarPartidas(System.Action<string> onPartidaId)
    {
        Log("[4/7] Listando partidas del usuario...");
        bool done = false;

        ApiSetup.Partida.ListarPartidas(
            lista =>
            {
                if (lista.Count == 0)
                {
                    LogWarning("El usuario no tiene partidas.");
                    onPartidaId(null);
                }
                else
                {
                    LogOk($"Partidas encontradas: {lista.Count}");
                    foreach (var p in lista)
                        Log($"  → [{p.id}] mapa={p.mapa_actual} combates={p.combates_ganados}W/{p.combates_perdidos}L");
                    onPartidaId(lista[0].id);
                }
                done = true;
            },
            err =>
            {
                LogError($"Error listando partidas: {err}");
                onPartidaId(null);
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── 5. Obtener equipo ─────────────────────────────────────────────────────

    private IEnumerator TestObtenerEquipo(string partidaId, System.Action<List<IbermonJugador>> onEquipo)
    {
        Log($"[5/7] Obteniendo equipo de la partida {partidaId}...");
        bool done = false;

        ApiSetup.IbermonJugador.ObtenerEquipo(partidaId,
            equipo =>
            {
                if (equipo.Count == 0)
                {
                    LogWarning("El equipo está vacío.");
                    onEquipo(equipo);
                }
                else
                {
                    LogOk($"Ibermon en el equipo: {equipo.Count}");
                    foreach (var ib in equipo)
                    {
                        Log($"  → [{ib.id}] catalogo_id={ib.ibermon_catalogo_id} " +
                            $"nivel={ib.nivel} hp={ib.hp_actual}");

                        // Verificar que movimientos_aprendidos tiene el nuevo formato {numero, pp}
                        if (ib.movimientos_aprendidos.Count > 0)
                        {
                            var movs = new System.Text.StringBuilder("    Movimientos: ");
                            foreach (var m in ib.movimientos_aprendidos)
                                movs.Append($"[#{m.numero} PP={m.pp}] ");
                            Log(movs.ToString());
                        }
                        else
                            Log("    (sin movimientos aprendidos)");
                    }
                    onEquipo(equipo);
                }
                done = true;
            },
            err =>
            {
                LogError($"Error obteniendo equipo: {err}");
                onEquipo(null);
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── 6. Converter ─────────────────────────────────────────────────────────

    private void TestConverter(List<IbermonJugador> equipo)
    {
        Log("[6/7] Probando IbermonConverter (API → Pokemon Unity)...");

        if (!CatalogoCache.Instance.EstaListo)
        {
            LogWarning("CatalogoCache no está listo, saltando converter.");
            return;
        }

        int ok = 0, fail = 0;
        foreach (var ib in equipo)
        {
            var pokemon = IbermonConverter.ToPokemon(ib, CatalogoCache.Instance);
            if (pokemon == null)
            {
                fail++;
                continue;
            }

            ok++;
            var movInfo = new System.Text.StringBuilder();
            foreach (var m in pokemon.Moves)
                movInfo.Append($"[{m.Base.Name} PP={m.PP}/{m.Base.PP}] ");

            Log($"  ✅ {pokemon.Base.Name} Nv.{pokemon.Level} HP={pokemon.HP}/{pokemon.MaxHp} | {movInfo}");
        }

        if (fail == 0)
            LogOk($"Converter OK: {ok}/{equipo.Count} ibermon convertidos.");
        else
            LogError($"Converter: {ok} OK, {fail} FALLOS (revisa que los ScriptableObjects " +
                     "en Resources/Pokemons/ tienen el mismo nombre que el catálogo).");
    }

    // ── 7. Sincronizar ibermon ────────────────────────────────────────────────

    private IEnumerator TestSincronizarIbermon(string partidaId, IbermonJugador ib)
    {
        Log($"[7/7] Sincronizando ibermon {ib.id} (test de escritura)...");
        bool done = false;

        // Convertir a Pokemon, simular 1 PP gastado en el primer movimiento, y re-sincronizar
        var pokemon = IbermonConverter.ToPokemon(ib, CatalogoCache.Instance);
        if (pokemon == null)
        {
            LogWarning("No se pudo convertir el ibermon, saltando test de sync.");
            yield break;
        }

        if (pokemon.Moves.Count > 0)
            pokemon.Moves[0].PP = Mathf.Max(0, pokemon.Moves[0].PP - 1);   // gastar 1 PP

        var request = IbermonConverter.ToActualizarRequest(pokemon, CatalogoCache.Instance);

        ApiSetup.IbermonJugador.ActualizarIbermon(partidaId, ib.id, request,
            updated =>
            {
                LogOk($"Sync OK — ibermon {updated.ibermon_catalogo_id} actualizado.");
                if (updated.movimientos_aprendidos.Count > 0)
                {
                    var movs = new System.Text.StringBuilder("  PP tras sync: ");
                    foreach (var m in updated.movimientos_aprendidos)
                        movs.Append($"[#{m.numero} PP={m.pp}] ");
                    Log(movs.ToString());
                }
                done = true;
            },
            err =>
            {
                LogError($"Error en sync: {err}");
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── Crear partida ─────────────────────────────────────────────────────────

    private IEnumerator TestCrearPartida(System.Action<string> onPartidaId)
    {
        Log($"[4b] No hay partidas — creando partida de prueba (personaje='{personajeElegido}', starter={starterElegido})...");
        bool done = false;

        ApiSetup.Partida.CrearPartida(personajeElegido, starterElegido,
            partida =>
            {
                LogOk($"Partida creada: [{partida.id}] mapa={partida.mapa_actual}");
                onPartidaId(partida.id);
                done = true;
            },
            err =>
            {
                LogError($"Error creando partida: {err}");
                onPartidaId(null);
                done = true;
            });

        yield return new WaitUntil(() => done);
    }

    // ── Helpers de log ────────────────────────────────────────────────────────

    private static void Log(string msg)        => Debug.Log($"<color=#aaaaaa>[TestAPI]</color> {msg}");
    private static void LogOk(string msg)      => Debug.Log($"<color=#55ff55>[TestAPI] ✅ {msg}</color>");
    private static void LogWarning(string msg) => Debug.LogWarning($"[TestAPI] ⚠️ {msg}");
    private static void LogError(string msg)   => Debug.LogError($"[TestAPI] ❌ {msg}");
}
