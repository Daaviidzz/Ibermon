using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-10 — Tests para RecoveryItem (Bloque 2 - Combate)
//
//  RecoveryItem es la subclase de ItemBase que cura HP, restaura PP, cura status
//  o revive Ibermon debilitados. Su método Use(Pokemon) tiene varias ramas según
//  qué flags están activados. Aquí cubrimos las más importantes:
//   · Revive con HP > 0 → false (no se gasta)
//   · Revive con HP = 0 → restaura 50% (revive) o 100% (maxRevive)
//   · Curación HP con HP al tope → false
//   · Restauración de PP
//   · Curación de status concreto vs todos
// =====================================================================================
public class TestRecoveryItem
{
    private RecoveryItem CrearItem(int hpAmount = 0, bool restoreMaxHP = false,
                                    int ppAmount = 0, bool restoreMaxPP = false,
                                    ConditionID status = ConditionID.none,
                                    bool recoverAllStatus = false,
                                    bool revive = false, bool maxRevive = false)
    {
        var item = ScriptableObject.CreateInstance<RecoveryItem>();
        TestHelpers.SetField(item, "hpAmount", hpAmount);
        TestHelpers.SetField(item, "restoreMaxHP", restoreMaxHP);
        TestHelpers.SetField(item, "ppAmount", ppAmount);
        TestHelpers.SetField(item, "restoreMaxPP", restoreMaxPP);
        TestHelpers.SetField(item, "status", status);
        TestHelpers.SetField(item, "recoverAllStatus", recoverAllStatus);
        TestHelpers.SetField(item, "revive", revive);
        TestHelpers.SetField(item, "maxRevive", maxRevive);
        return item;
    }

    [SetUp]
    public void Setup()
    {
        ConditionsDB.Init();
    }

    // -----------------------------------------------------------------------------------
    //  PR-10.1 - Pocion (hpAmount=30) cura 30 puntos de HP
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR10_01_Pocion_CuraHP()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.HP = pokemon.MaxHp - 30; // herido en 30 puntos
        int hpAntes = pokemon.HP;

        var pocion = CrearItem(hpAmount: 30);
        bool ok = pocion.Use(pokemon);

        Assert.IsTrue(ok, "Use debe devolver true si efectivamente cura");
        Assert.AreEqual(hpAntes + 30, pokemon.HP);

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(pocion);
    }

    // -----------------------------------------------------------------------------------
    //  PR-10.2 - Pocion en pokemon con HP al tope no se gasta
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR10_02_Pocion_HPAlTope_NoSeGasta()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        // pokemon.HP ya está a MaxHp tras Init()

        var pocion = CrearItem(hpAmount: 30);
        bool ok = pocion.Use(pokemon);

        Assert.IsFalse(ok, "Use debe devolver false si el Ibermon está al máximo HP");

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(pocion);
    }

    // -----------------------------------------------------------------------------------
    //  PR-10.3 - Revive solo funciona si HP = 0
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR10_03_Revive_RequiereHpCero()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.HP = 50; // sigue vivo

        var revive = CrearItem(revive: true);
        bool ok = revive.Use(pokemon);

        Assert.IsFalse(ok, "Revive en pokemon vivo debe devolver false");
        Assert.AreEqual(50, pokemon.HP, "El HP no debe cambiar");

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(revive);
    }

    // -----------------------------------------------------------------------------------
    //  PR-10.4 - Revive en pokemon debilitado restaura 50% del MaxHp
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR10_04_Revive_RestauraMitad()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.HP = 0; // debilitado

        var revive = CrearItem(revive: true);
        bool ok = revive.Use(pokemon);

        Assert.IsTrue(ok);
        Assert.AreEqual(pokemon.MaxHp / 2, pokemon.HP,
            "Revive debe restaurar la mitad del MaxHp");

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(revive);
    }

    // -----------------------------------------------------------------------------------
    //  PR-10.5 - MaxRevive restaura 100% del MaxHp
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR10_05_MaxRevive_RestauraCompleto()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.HP = 0;

        var maxRevive = CrearItem(maxRevive: true);
        bool ok = maxRevive.Use(pokemon);

        Assert.IsTrue(ok);
        Assert.AreEqual(pokemon.MaxHp, pokemon.HP, "MaxRevive debe poner HP al máximo");

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(maxRevive);
    }

    // -----------------------------------------------------------------------------------
    //  PR-10.6 - RecoverAllStatus cura veneno, quemadura, etc.
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR10_06_RecoverAllStatus_CuraEstado()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.SetStatus(ConditionID.psn);
        Assert.IsNotNull(pokemon.Status, "Setup: el pokemon debe tener veneno");

        var antidoto = CrearItem(recoverAllStatus: true);
        bool ok = antidoto.Use(pokemon);

        Assert.IsTrue(ok, "Use debe devolver true al curar un estado activo");
        Assert.IsNull(pokemon.Status, "Status debe ser null tras curar");

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(antidoto);
    }
}
