using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-17 — Tests para Interactuable y FaseDialogo (Bloque 3 - Mapa/NPCs)
//
//  Interactuable es el componente que se añade a cualquier NPC y gestiona los
//  diálogos por fases. Cada NPC tiene una lista fasesDialogo y un campo
//  faseActual que indica cuál se muestra. CambiarFase(n) permite cambiar de
//  fase desde código.
//
//  No podemos testear toda la lógica de diálogo (depende de UI, animaciones,
//  Rigidbody y tecla), pero sí los aspectos lógicos básicos del sistema de fases.
// =====================================================================================
public class TestInteractuable
{
    private Interactuable _npc;
    private GameObject _go;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("NPCTest");
        // SetActive(false) para que Awake/Start no se ejecuten con datos vacíos
        _go.SetActive(false);
        _npc = _go.AddComponent<Interactuable>();

        // Configuramos 3 fases con frases distintas
        _npc.fasesDialogo = new List<FaseDialogo>
        {
            new FaseDialogo
            {
                textosDeEstaFase = new List<string>
                {
                    "¡Hola! Soy un NPC en fase 0.",
                    "Captura más Ibermon!"
                }
            },
            new FaseDialogo
            {
                textosDeEstaFase = new List<string>
                {
                    "Ya tienes muchos Ibermon, estoy orgulloso!"
                }
            },
            new FaseDialogo
            {
                textosDeEstaFase = new List<string>
                {
                    "Eres el campeón!"
                }
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  PR-17.1 - faseActual inicial es 0
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR17_01_FaseActual_InicialCero()
    {
        Assert.AreEqual(0, _npc.faseActual,
            "Un NPC nuevo debe empezar en fase 0");
    }

    // -----------------------------------------------------------------------------------
    //  PR-17.2 - CambiarFase con índice válido actualiza faseActual
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR17_02_CambiarFase_IndiceValido_Cambia()
    {
        _npc.CambiarFase(1);
        Assert.AreEqual(1, _npc.faseActual, "CambiarFase(1) debe poner faseActual a 1");

        _npc.CambiarFase(2);
        Assert.AreEqual(2, _npc.faseActual, "CambiarFase(2) debe poner faseActual a 2");
    }

    // -----------------------------------------------------------------------------------
    //  PR-17.3 - CambiarFase con índice fuera de rango NO cambia faseActual
    //  Es la protección del método para no romper el array.
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR17_03_CambiarFase_IndiceInvalido_NoCambia()
    {
        _npc.faseActual = 1;

        _npc.CambiarFase(99); // fuera de rango (solo hay 3 fases)
        Assert.AreEqual(1, _npc.faseActual,
            "Con índice fuera de rango (99) no debe cambiar la fase");

        _npc.CambiarFase(-1); // negativo
        Assert.AreEqual(1, _npc.faseActual,
            "Con índice negativo (-1) no debe cambiar la fase");
    }

    // -----------------------------------------------------------------------------------
    //  PR-17.4 - FaseDialogo guarda lista de textos correctamente
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR17_04_FaseDialogo_GuardaListaDeTextos()
    {
        var fase = _npc.fasesDialogo[0];
        Assert.AreEqual(2, fase.textosDeEstaFase.Count,
            "La fase 0 debería tener 2 frases configuradas");
        Assert.AreEqual("¡Hola! Soy un NPC en fase 0.", fase.textosDeEstaFase[0]);
        Assert.AreEqual("Captura más Ibermon!", fase.textosDeEstaFase[1]);
    }

    // -----------------------------------------------------------------------------------
    //  PR-17.5 - Campos públicos por defecto tienen valores razonables
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR17_05_ValoresPorDefecto_RazonablesParaNPC()
    {
        var nuevo = new GameObject("NPCNuevo");
        nuevo.SetActive(false);
        var npcNuevo = nuevo.AddComponent<Interactuable>();

        Assert.IsFalse(npcNuevo.esEntrenador,
            "Por defecto un NPC NO es entrenador");
        Assert.AreEqual("Pulsa E para interactuar", npcNuevo.mensaje,
            "El mensaje por defecto es el de interacción");
        Assert.IsFalse(npcNuevo.cambiarFaseSegunPokemon,
            "Por defecto el cambio automático de fase está desactivado");
        Assert.AreEqual(5, npcNuevo.pokemonNecesariosParaFase1,
            "Por defecto se requieren 5 Ibermon para fase 1");

        Object.DestroyImmediate(nuevo);
    }
}
