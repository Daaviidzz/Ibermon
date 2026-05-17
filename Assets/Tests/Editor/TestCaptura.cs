using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-11 — Tests para PokeballItem y fórmula de captura (Bloque 2 - Combate)
//
//  PokeballItem.Use(Pokemon) solo devuelve true si la escena activa es "Combate".
//  Esto es para evitar que el jugador "use" una pokéball desde el inventario
//  estando en el mundo (no tiene sentido).
//
//  Adicionalmente, la fórmula de captura está implementada en BattleSystem.
//  Como no podemos instanciar BattleSystem sin escena, replicamos la fórmula
//  aquí y validamos los casos límite (HP muy bajo → captura segura, HP al tope
//  con bajo catchRate → captura difícil).
// =====================================================================================
public class TestCaptura
{
    [SetUp]
    public void Setup()
    {
        ConditionsDB.Init();
    }

    // -----------------------------------------------------------------------------------
    //  Réplica de la fórmula de captura (BattleSystem.TryCatchPokemon)
    //  Devuelve el número de "sacudidas" (0-4). 4 = captura exitosa.
    // -----------------------------------------------------------------------------------
    private int TryCatchPokemon(Pokemon pokemon, float pokeballMod)
    {
        int catchRate = 100; // valor de prueba, en la fórmula real viene del PokemonBase
        var statusBonus = ConditionsDB.GetStatusBonus(pokemon.Status);

        float a = ((3 * pokemon.MaxHp) - (2 * pokemon.HP))
                  * catchRate * statusBonus * pokeballMod
                  / (3 * pokemon.MaxHp);

        if (a >= 255) return 4; // captura segura

        float b = 1048560f / Mathf.Sqrt(Mathf.Sqrt(16711680f / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (Random.Range(0, 65535) >= b) break;
            ++shakeCount;
        }
        return shakeCount;
    }

    // -----------------------------------------------------------------------------------
    //  PR-11.1 - PokeballItem.Use fuera de combate devuelve false
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR11_01_PokeballItem_FueraDeCombate_DevuelveFalse()
    {
        var pokeball = ScriptableObject.CreateInstance<PokeballItem>();

        var pb = TestHelpers.CrearPokemonBase();
        var pokemon = new Pokemon(pb, pLevel: 10);

        // La escena activa en EditMode es la que esté abierta, NO "Combate"
        bool resultado = pokeball.Use(pokemon);

        Assert.IsFalse(resultado,
            "PokeballItem.Use() solo debe devolver true si la escena activa es \"Combate\"");

        Object.DestroyImmediate(pb);
        Object.DestroyImmediate(pokeball);
    }

    // -----------------------------------------------------------------------------------
    //  PR-11.2 - Fórmula captura con HP muy bajo → tasa significativa (>25%)
    //  Nota: con catchRate=100 y HP=1, la fórmula da ~35% de captura por intento.
    //  Una "captura casi segura" requiere catchRate alto (255) o un Ibermon dormido.
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR11_02_FormulaCaptura_HpMuyBajo_AltaTasa()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        pokemon.HP = 1; // moribundo

        int capturas = 0;
        int iteraciones = 200;
        Random.InitState(42);

        for (int i = 0; i < iteraciones; i++)
        {
            if (TryCatchPokemon(pokemon, pokeballMod: 1f) == 4)
                capturas++;
        }

        float tasaCaptura = (float)capturas / iteraciones;
        Assert.Greater(tasaCaptura, 0.25f,
            $"Con HP=1 y catchRate=100, la tasa de captura debe ser >25%. Resultado: {tasaCaptura:P}");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-11.3 - Fórmula captura con HP al tope → captura difícil
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR11_03_FormulaCaptura_HpAlTope_BajaTasa()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);
        var pokemon = new Pokemon(pb, pLevel: 50);
        // pokemon.HP ya está a MaxHp tras Init()

        int capturas = 0;
        int iteraciones = 200;
        Random.InitState(42);

        for (int i = 0; i < iteraciones; i++)
        {
            if (TryCatchPokemon(pokemon, pokeballMod: 1f) == 4)
                capturas++;
        }

        float tasaCaptura = (float)capturas / iteraciones;
        Assert.Less(tasaCaptura, 0.20f,
            $"Con HP=MaxHp la tasa de captura debe ser <20%. Resultado: {tasaCaptura:P}");

        Object.DestroyImmediate(pb);
    }

    // -----------------------------------------------------------------------------------
    //  PR-11.4 - Status bonus mejora la captura: el sueño duplica la efectividad
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR11_04_StatusBonus_MejoraCaptura()
    {
        var pb = TestHelpers.CrearPokemonBase(maxHp: 100);

        // Sin status
        var pokemonSinStatus = new Pokemon(pb, pLevel: 50);
        pokemonSinStatus.HP = 30;

        // Con sueño
        var pokemonDormido = new Pokemon(pb, pLevel: 50);
        pokemonDormido.HP = 30;
        pokemonDormido.SetStatus(ConditionID.slp);

        int capturasSinStatus = 0, capturasDormido = 0;
        int iteraciones = 300;

        Random.InitState(123);
        for (int i = 0; i < iteraciones; i++)
            if (TryCatchPokemon(pokemonSinStatus, 1f) == 4) capturasSinStatus++;

        Random.InitState(123);
        for (int i = 0; i < iteraciones; i++)
            if (TryCatchPokemon(pokemonDormido, 1f) == 4) capturasDormido++;

        Assert.Greater(capturasDormido, capturasSinStatus,
            $"El sueño debe mejorar la captura. Sin status: {capturasSinStatus}, dormido: {capturasDormido}");

        Object.DestroyImmediate(pb);
    }
}
