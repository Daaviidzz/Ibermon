using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-08 — Tests para ConditionsDB (Bloque 2 - Combate)
//
//  ConditionsDB es la "base de datos" en memoria de las 6 condiciones de estado:
//  veneno (psn), quemadura (brn), sueño (slp), parálisis (par), congelado (frz)
//  y confusión. Cada condición tiene delegados que se ejecutan en distintos puntos
//  del turno: OnStart (al aplicarla), OnBeforeMove (antes del movimiento) y
//  OnAfterTurn (al final del turno).
//
//  Aquí testeamos los efectos más importantes:
//   · Veneno hace MaxHp/8 de daño por turno
//   · Quemadura hace MaxHp/16 por turno
//   · GetStatusBonus aplica los multiplicadores correctos para captura
// =====================================================================================
public class TestConditionsDB
{
    [SetUp]
    public void Setup()
    {
        // ConditionsDB.Init() asigna los Ids a cada Condition del diccionario
        ConditionsDB.Init();
    }

    // -----------------------------------------------------------------------------------
    //  PR-08.1 - Veneno (psn) reduce HP en MaxHp/8 cada turno
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR08_01_Veneno_DanoOctavoPorTurno()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 80);
        var pokemon = new Pokemon(pb, pLevel: 10);
        // MaxHp final = floor(80*10/100) + 10 + 10 = 28. Daño esperado = 28/8 = 3
        int hpAntes = pokemon.HP;
        int danoEsperado = pokemon.MaxHp / 8;

        pokemon.SetStatus(ConditionID.psn);
        pokemon.OnAfterTurn();

        int danoReal = hpAntes - pokemon.HP;
        Assert.AreEqual(danoEsperado, danoReal,
            $"Veneno debe causar MaxHp/8 = {danoEsperado} de daño");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-08.2 - Quemadura (brn) reduce HP en MaxHp/16 cada turno
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR08_02_Quemadura_DanoDieciseisavoPorTurno()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 160);
        var pokemon = new Pokemon(pb, pLevel: 10);
        int hpAntes = pokemon.HP;
        int danoEsperado = pokemon.MaxHp / 16;

        pokemon.SetStatus(ConditionID.brn);
        pokemon.OnAfterTurn();

        int danoReal = hpAntes - pokemon.HP;
        Assert.AreEqual(danoEsperado, danoReal,
            $"Quemadura debe causar MaxHp/16 = {danoEsperado} de daño");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-08.3 - Veneno y quemadura encolan mensaje en StatusChanges
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR08_03_StatusChanges_SeRellenanMensajes()
    {
        var pb = TestHelpers.CrearPokemonBase("TestMon");
        var pokemon = new Pokemon(pb, pLevel: 10);

        pokemon.SetStatus(ConditionID.psn);
        pokemon.OnAfterTurn();

        Assert.GreaterOrEqual(pokemon.StatusChanges.Count, 1,
            "Tras SetStatus + OnAfterTurn debe haber al menos un mensaje en StatusChanges");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-08.4 - Diccionario Conditions contiene las 6 condiciones (sin contar 'none')
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR08_04_Diccionario_Contiene6Condiciones()
    {
        Assert.IsTrue(ConditionsDB.Conditions.ContainsKey(ConditionID.psn),
            "Falta veneno (psn) en el diccionario");
        Assert.IsTrue(ConditionsDB.Conditions.ContainsKey(ConditionID.brn),
            "Falta quemadura (brn)");
        Assert.IsTrue(ConditionsDB.Conditions.ContainsKey(ConditionID.slp),
            "Falta sueño (slp)");
        Assert.IsTrue(ConditionsDB.Conditions.ContainsKey(ConditionID.par),
            "Falta parálisis (par)");
        Assert.IsTrue(ConditionsDB.Conditions.ContainsKey(ConditionID.frz),
            "Falta congelado (frz)");
        Assert.IsTrue(ConditionsDB.Conditions.ContainsKey(ConditionID.confusion),
            "Falta confusión");
    }

    // -----------------------------------------------------------------------------------
    //  PR-08.5 - GetStatusBonus aplica los multiplicadores correctos
    //  sueño/congelado → 2×, parálisis/veneno/quemadura → 1.5×, resto → 1×
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR08_05_GetStatusBonus_Multiplicadores()
    {
        Assert.AreEqual(2f, ConditionsDB.GetStatusBonus(ConditionsDB.Conditions[ConditionID.slp]), 0.01f,
            "Sueño debe dar bonus 2× a la captura");
        Assert.AreEqual(2f, ConditionsDB.GetStatusBonus(ConditionsDB.Conditions[ConditionID.frz]), 0.01f,
            "Congelado debe dar bonus 2×");
        Assert.AreEqual(1.5f, ConditionsDB.GetStatusBonus(ConditionsDB.Conditions[ConditionID.par]), 0.01f,
            "Parálisis debe dar bonus 1.5×");
        Assert.AreEqual(1.5f, ConditionsDB.GetStatusBonus(ConditionsDB.Conditions[ConditionID.psn]), 0.01f,
            "Veneno debe dar bonus 1.5×");
        Assert.AreEqual(1.5f, ConditionsDB.GetStatusBonus(ConditionsDB.Conditions[ConditionID.brn]), 0.01f,
            "Quemadura debe dar bonus 1.5×");
        Assert.AreEqual(1f, ConditionsDB.GetStatusBonus(null), 0.01f,
            "Sin estado, bonus es 1× (sin modificador)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-08.6 - Parálisis: en N tiradas debe bloquear aproximadamente 1/4 del tiempo
    //  Como usa Random, hacemos muchas iteraciones y tolerancia ancha (±10%)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR08_06_Paralisis_BloqueaUnCuarto()
    {
        var pb = TestHelpers.CrearPokemonBase();
        var pokemon = new Pokemon(pb, pLevel: 10);
        pokemon.SetStatus(ConditionID.par);

        int bloqueados = 0;
        int iteraciones = 2000;

        Random.InitState(42); // semilla fija para reproducibilidad
        for (int i = 0; i < iteraciones; i++)
        {
            if (!pokemon.OnBeforeMove()) bloqueados++;
        }

        float porcentajeBloqueo = (float)bloqueados / iteraciones;
        Assert.That(porcentajeBloqueo, Is.InRange(0.15f, 0.35f),
            $"Parálisis debe bloquear ~25% de los turnos. Resultado: {porcentajeBloqueo:P}");

        Object.DestroyImmediate(pb);
    }
}
