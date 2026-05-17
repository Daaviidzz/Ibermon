using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-12 — Tests para PokemonParty (Bloque 2 - Combate)
//
//  PokemonParty representa el equipo activo de Ibermon del jugador o de un
//  entrenador rival. Aquí testeamos las operaciones más importantes:
//   · SetPokemonsForBattle inicializa el equipo (usado por TrainerController)
//   · GetHealtyPokemon devuelve el primer Ibermon con HP > 0
//   · AddPokemon añade hasta máximo 6 (regla clásica)
//   · HealAllPokemonsInParty cura todo el equipo (usado por la abuela)
// =====================================================================================
public class TestPokemonParty
{
    private PokemonParty _party;
    private GameObject _go;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("PokemonPartyTest");
        // SetActive(false) antes de añadir el componente para evitar que Start()
        // se ejecute con datos vacíos (este es el mismo truco que usa BattleSystem
        // al crear el trainerParty temporal).
        _go.SetActive(false);
        _party = _go.AddComponent<PokemonParty>();

        // No marcamos como esEquipoJugador para evitar la carga automática
        var fEsJugador = typeof(PokemonParty).GetField("esEquipoJugador",
            BindingFlags.NonPublic | BindingFlags.Instance);
        fEsJugador?.SetValue(_party, false);

        _go.SetActive(true);

        ConditionsDB.Init();
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  PR-12.1 - SetPokemonsForBattle asigna la lista al equipo
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR12_01_SetPokemonsForBattle_AsignaLista()
    {
        var pb = TestHelpers.CrearPokemonBase();
        var lista = new List<Pokemon>
        {
            new Pokemon(pb, pLevel: 5),
            new Pokemon(pb, pLevel: 7)
        };

        _party.SetPokemonsForBattle(lista);

        Assert.AreEqual(2, _party.Pokemons.Count);

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-12.2 - GetHealtyPokemon devuelve el primero con HP > 0
    //  Importante: SetPokemonsForBattle llama a p.Init() en cada Pokemon, que
    //  resetea HP=MaxHp. Por eso debemos poner HP=0 DESPUÉS de añadirlos al party.
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR12_02_GetHealtyPokemon_DevuelvePrimeroVivo()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 50);

        var muerto = new Pokemon(pb, pLevel: 10);
        var vivo = new Pokemon(pb, pLevel: 10);

        _party.SetPokemonsForBattle(new List<Pokemon> { muerto, vivo });

        // Init() durante SetPokemonsForBattle reseteó el HP; lo bajamos a 0 ahora.
        muerto.HP = 0;

        var seleccionado = _party.GetHealtyPokemon();

        Assert.AreSame(vivo, seleccionado,
            "GetHealtyPokemon debe saltarse los debilitados y devolver el primero vivo");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-12.3 - AddPokemon añade al equipo si hay hueco (max 6)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR12_03_AddPokemon_AnadeSiHayHueco()
    {
        var pb = TestHelpers.CrearPokemonBase();
        _party.SetPokemonsForBattle(new List<Pokemon> { new Pokemon(pb, 10) });

        bool ok = _party.AddPokemon(new Pokemon(pb, 12));

        Assert.IsTrue(ok, "AddPokemon debe devolver true cuando hay hueco");
        Assert.AreEqual(2, _party.Pokemons.Count);

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-12.4 - AddPokemon rechaza si el equipo ya tiene 6 (regla clásica)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR12_04_AddPokemon_RechazaConEquipoLleno()
    {
        var pb = TestHelpers.CrearPokemonBase();
        var lista = new List<Pokemon>();
        for (int i = 0; i < 6; i++) lista.Add(new Pokemon(pb, 10));
        _party.SetPokemonsForBattle(lista);

        bool ok = _party.AddPokemon(new Pokemon(pb, 12));

        Assert.IsFalse(ok, "Con 6 Ibermon el equipo está lleno, AddPokemon debe devolver false");
        Assert.AreEqual(6, _party.Pokemons.Count);

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-12.5 - HealAllPokemonsInParty cura HP y elimina status de todos
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR12_05_HealAllPokemonsInParty_CuraTodo()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var p1 = new Pokemon(pb, pLevel: 10);
        var p2 = new Pokemon(pb, pLevel: 10);
        p1.HP = 5;
        p2.HP = 10;
        p2.SetStatus(ConditionID.psn);

        _party.SetPokemonsForBattle(new List<Pokemon> { p1, p2 });

        _party.HealAllPokemonsInParty();

        Assert.AreEqual(p1.MaxHp, p1.HP, "p1 debe tener HP al máximo");
        Assert.AreEqual(p2.MaxHp, p2.HP, "p2 debe tener HP al máximo");
        Assert.IsNull(p2.Status, "El veneno de p2 debe estar curado");

        Object.DestroyImmediate(pb);
    }
}
