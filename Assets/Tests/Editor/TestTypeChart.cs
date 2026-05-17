using NUnit.Framework;

// =====================================================================================
//  PR-07 — Tests para TypeChart (Bloque 2 - Combate)
//
//  TypeChart es una matriz estática 15×15 con los multiplicadores de daño entre
//  tipos. Aquí comprobamos algunas combinaciones clave conocidas:
//   · Agua → Fuego = 2× (super-efectivo)
//   · Fuego → Agua = 0.5× (poco eficaz)
//   · Normal → Fantasma = 0× (inmune)
//   · Eléctrico → Tierra = 0× (inmune)
//   · Planta → Agua = 2× (super-efectivo)
//   · Tipo desconocido → tipo cualquiera = 1× (sin modificador)
// =====================================================================================
public class TestTypeChart
{
    // -----------------------------------------------------------------------------------
    //  PR-07.1 - Agua super-efectivo contra Fuego (2×)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR07_01_AguaContraFuego_2x()
    {
        float efectividad = TypeChart.GetEffectiveness(PokemonType.Agua, PokemonType.Fuego);
        Assert.AreEqual(2f, efectividad, 0.01f,
            "Agua contra Fuego debe ser 2× (super-efectivo)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-07.2 - Fuego poco eficaz contra Agua (0.5×)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR07_02_FuegoContraAgua_MediaEfectividad()
    {
        float efectividad = TypeChart.GetEffectiveness(PokemonType.Fuego, PokemonType.Agua);
        Assert.AreEqual(0.5f, efectividad, 0.01f,
            "Fuego contra Agua debe ser 0.5× (poco eficaz)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-07.3 - Normal contra Fantasma es inmune (0×)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR07_03_NormalContraFantasma_Inmune()
    {
        float efectividad = TypeChart.GetEffectiveness(PokemonType.Normal, PokemonType.Fantasma);
        Assert.AreEqual(0f, efectividad, 0.01f,
            "Normal contra Fantasma debe ser 0× (inmune)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-07.4 - Eléctrico contra Tierra es inmune (0×)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR07_04_ElectricoContraTierra_Inmune()
    {
        float efectividad = TypeChart.GetEffectiveness(PokemonType.Electrico, PokemonType.Tierra);
        Assert.AreEqual(0f, efectividad, 0.01f,
            "Eléctrico contra Tierra debe ser 0× (inmune)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-07.5 - Planta super-efectivo contra Agua (2×)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR07_05_PlantaContraAgua_2x()
    {
        float efectividad = TypeChart.GetEffectiveness(PokemonType.Planta, PokemonType.Agua);
        Assert.AreEqual(2f, efectividad, 0.01f,
            "Planta contra Agua debe ser 2× (super-efectivo)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-07.6 - Tipo None devuelve 1× (no aplica multiplicador) — para el segundo
    //  tipo de los ibermon monotipo.
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR07_06_TipoNone_DevuelveUno()
    {
        // Esto se usa cuando un ibermon es monotipo: Type2 = None, y el cálculo de
        // efectividad multiplica por TypeChart.GetEffectiveness(moveType, None).
        // Debe devolver 1f para no afectar al cálculo.
        float ef = TypeChart.GetEffectiveness(PokemonType.Fuego, PokemonType.None);
        Assert.AreEqual(1f, ef, 0.01f,
            "Cuando defType2 = None, GetEffectiveness debe devolver 1× (neutro)");

        float ef2 = TypeChart.GetEffectiveness(PokemonType.None, PokemonType.Agua);
        Assert.AreEqual(1f, ef2, 0.01f,
            "Cuando attackType = None, también debe ser 1×");
    }
}
