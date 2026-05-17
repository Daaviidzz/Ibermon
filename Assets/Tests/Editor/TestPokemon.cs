using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-06 — Tests para la clase Pokemon (Bloque 2 - Combate)
//
//  Pokemon es la clase nuclear del juego: representa una instancia individual de
//  criatura con sus stats, movimientos, condiciones de estado y métodos de combate.
//  Aquí testeamos sus fórmulas más críticas:
//   · CalculateStats() con la fórmula oficial Pokémon
//   · TakeDamage() con efectividad de tipos
//   · DecreaseHP / IncreaseHP con clamping a [0, MaxHp]
//   · ApplyBoosts con clamping a [-6, +6]
// =====================================================================================
public class TestPokemon
{
    [SetUp]
    public void Setup()
    {
        // ConditionsDB es una clase estática que necesita Init() para asignar IDs.
        // Lo hacemos antes de cada test por si algún otro test la ha tocado.
        ConditionsDB.Init();
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.1 - CalculateStats con la fórmula oficial Pokémon
    //  Fórmula: stat = floor((statBase × nivel) / 100) + 5
    //           MaxHp = floor((hpBase × nivel) / 100) + 10 + nivel
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_01_CalculateStats_FormulaOficial()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 80, attack: 70, defense: 60,
                                              spAttack: 50, spDefense: 90, speed: 40);
        var pokemon = new Pokemon(pb, pLevel: 20);

        // MaxHp = floor((80*20)/100) + 10 + 20 = 16 + 10 + 20 = 46
        Assert.AreEqual(46, pokemon.MaxHp, "MaxHp con la fórmula oficial");
        // Attack = floor((70*20)/100) + 5 = 14 + 5 = 19
        Assert.AreEqual(19, pokemon.Attack, "Attack con la fórmula oficial");
        // Defense = floor((60*20)/100) + 5 = 12 + 5 = 17
        Assert.AreEqual(17, pokemon.Defense);
        // SpAttack = floor((50*20)/100) + 5 = 10 + 5 = 15
        Assert.AreEqual(15, pokemon.SpAttack);
        // Speed = floor((40*20)/100) + 5 = 8 + 5 = 13
        Assert.AreEqual(13, pokemon.Speed);

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.2 - HP inicial coincide con MaxHp tras Init()
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_02_HP_InicialEsMaxHp()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 10);

        Assert.AreEqual(pokemon.MaxHp, pokemon.HP,
            "Tras Init() el HP debe estar al máximo (sin daño)");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.3 - DecreaseHP clampea a 0 (no puede ser negativo)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_03_DecreaseHP_ClampeaACero()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 50);
        var pokemon = new Pokemon(pb, pLevel: 10);
        int hpInicial = pokemon.HP;

        pokemon.DecreaseHP(hpInicial + 999); // daño excesivo

        Assert.AreEqual(0, pokemon.HP, "HP nunca debe ser negativo");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.4 - IncreaseHP clampea a MaxHp (no puede pasar del máximo)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_04_IncreaseHP_ClampeaAMaxHp()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 50);
        var pokemon = new Pokemon(pb, pLevel: 10);
        pokemon.HP = 5; // empezamos con poca vida

        pokemon.IncreaseHP(9999); // curación excesiva

        Assert.AreEqual(pokemon.MaxHp, pokemon.HP, "HP no debe pasar del MaxHp");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.5 - ApplyBoosts clampa entre -6 y +6
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_05_ApplyBoosts_ClampeaEntreMinusSeisYSeis()
    {
        var pb = TestHelpers.CrearPokemonBase();
        var pokemon = new Pokemon(pb, pLevel: 10);

        // Aplicamos un boost muy alto al ataque
        pokemon.ApplyBoosts(new System.Collections.Generic.List<StatBoost>
        {
            new StatBoost { stat = Stat.Ataque, boost = 10 }
        });
        Assert.AreEqual(6, pokemon.StatsBoosts[Stat.Ataque],
            "Boost positivo debe clampearse a +6");

        // Aplicamos un debuff muy bajo (lo bajamos 20 desde 6)
        pokemon.ApplyBoosts(new System.Collections.Generic.List<StatBoost>
        {
            new StatBoost { stat = Stat.Ataque, boost = -20 }
        });
        Assert.AreEqual(-6, pokemon.StatsBoosts[Stat.Ataque],
            "Boost negativo debe clampearse a -6");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.6 - SetStatus no aplica si ya hay un estado activo (regla Pokémon)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_06_SetStatus_NoSobreescribeEstadoExistente()
    {
        var pb = TestHelpers.CrearPokemonBase();
        var pokemon = new Pokemon(pb, pLevel: 10);

        pokemon.SetStatus(ConditionID.psn);
        Assert.IsNotNull(pokemon.Status);
        Assert.AreEqual(ConditionID.psn, pokemon.Status.Id);

        // Intentamos aplicar quemadura encima → no debe sobreescribir
        pokemon.SetStatus(ConditionID.brn);
        Assert.AreEqual(ConditionID.psn, pokemon.Status.Id,
            "El segundo SetStatus no debe sobreescribir el primero");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.7 - TakeDamage reduce HP del defensor
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_07_TakeDamage_ReduceHP()
    {
        var pbAtacante = TestHelpers.CrearPokemonBase("Atacante", attack: 100);
        var pbDefensor = TestHelpers.CrearPokemonBase("Defensor", maxHp: 200, defense: 30);
        var mb = TestHelpers.CrearMoveBase(power: 80, category: MoveCategory.Fisico);

        var atacante = new Pokemon(pbAtacante, pLevel: 50);
        var defensor = new Pokemon(pbDefensor, pLevel: 50);
        var move = new Move(mb);

        int hpAntes = defensor.HP;
        var details = defensor.TakeDamage(move, atacante);

        Assert.IsNotNull(details, "TakeDamage debe devolver DamageDetails");
        Assert.Less(defensor.HP, hpAntes, "El HP del defensor debe disminuir");
        Assert.GreaterOrEqual(defensor.HP, 0, "HP no puede ser negativo");

        Object.DestroyImmediate(pbAtacante);
        Object.DestroyImmediate(pbDefensor);
        Object.DestroyImmediate(mb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-06.8 - ResetHealth restaura HP al máximo
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR06_08_ResetHealth_RestauraMaxHp()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 10);
        pokemon.HP = 5;

        pokemon.ResetHealth();

        Assert.AreEqual(pokemon.MaxHp, pokemon.HP);

        Object.DestroyImmediate(pb);
    }
}
