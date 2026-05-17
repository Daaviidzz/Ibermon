using System.Collections.Generic;
using NUnit.Framework;
using ApiRest.Models;
using UnityEngine;

// =====================================================================================
//  PR-03 — Tests para SessionManager (Bloque 1 - API REST)
//
//  SessionManager mantiene el estado de la sesión: la partida activa, el equipo
//  recibido de la API, contadores de combate y tiempo jugado. Cualquier escena
//  del juego puede consultarlo vía SessionManager.Instance.
// =====================================================================================
public class TestSessionManager
{
    private SessionManager _session;
    private GameObject _go;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("SessionManagerTest");
        _session = _go.AddComponent<SessionManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  PR-03.1 - Estado inicial: TienePartida = false, PartidaId vacío
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR03_01_EstadoInicial_SinPartida()
    {
        Assert.IsFalse(_session.TienePartida,
            "Recién creado, SessionManager NO debe tener partida activa");
        Assert.IsTrue(string.IsNullOrEmpty(_session.PartidaId),
            "PartidaId debe estar vacío al inicio");
        Assert.AreEqual(0, _session.CombatesGanados);
        Assert.AreEqual(0, _session.CombatesPerdidos);
    }

    // -----------------------------------------------------------------------------------
    //  PR-03.2 - IniciarConPartida configura los datos correctamente
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR03_02_IniciarConPartida_ConfiguraDatos()
    {
        var partida = new PartidaCompleta
        {
            id = "partida_test_123",
            nombre = "Mi aventura",
            personaje_elegido = "torrente",
            tiempo_jugado = 3600,
            combates_ganados = 5,
            combates_perdidos = 2
        };
        var equipo = new List<IbermonJugador>
        {
            new IbermonJugador { ibermon_catalogo_id = 25, nivel = 10, hp_actual = 30 }
        };

        _session.IniciarConPartida(partida, equipo);

        Assert.IsTrue(_session.TienePartida, "Tras IniciarConPartida debe haber partida");
        Assert.AreEqual("partida_test_123", _session.PartidaId);
        Assert.AreEqual("Mi aventura", _session.PartidaActual.nombre);
        Assert.AreEqual(5, _session.CombatesGanados);
        Assert.AreEqual(2, _session.CombatesPerdidos);
        Assert.AreEqual(1, _session.EquipoAPI.Count);
    }

    // -----------------------------------------------------------------------------------
    //  PR-03.3 - IniciarConPartida con null hace LogError y no crashea
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR03_03_IniciarConPartida_Null_NoLanzaExcepcion()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex("No se puede iniciar sesion"));

        Assert.DoesNotThrow(() => _session.IniciarConPartida(null, null),
            "IniciarConPartida(null) debe loguear error pero no lanzar excepción");
        Assert.IsFalse(_session.TienePartida);
    }

    // -----------------------------------------------------------------------------------
    //  PR-03.4 - Equipo vacío al iniciar con lista null se acepta como lista vacía
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR03_04_EquipoNull_SeTrataComoListaVacia()
    {
        var partida = new PartidaCompleta { id = "p1", nombre = "Test" };

        _session.IniciarConPartida(partida, null);

        Assert.IsNotNull(_session.EquipoAPI, "EquipoAPI no debe ser null aunque le pasemos null");
        Assert.AreEqual(0, _session.EquipoAPI.Count, "Debe quedar como lista vacía");
    }
}
