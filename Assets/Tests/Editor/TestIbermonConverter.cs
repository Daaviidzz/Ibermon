using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ApiRest.Models;
using UnityEngine;

// =====================================================================================
//  PR-01 — Tests para IbermonConverter (Bloque 1 - API REST)  [v2 corregido]
//
//  Comprueba que la conversión entre los modelos de la API (IbermonJugador) y
//  los objetos del juego (Pokemon) funciona correctamente y que los casos de
//  error (catálogo no disponible, ibermon desconocido) se manejan sin crashes.
// =====================================================================================
public class TestIbermonConverter
{
    // -----------------------------------------------------------------------------------
    //  Helper: setea un campo privado de cualquier objeto por reflection
    // -----------------------------------------------------------------------------------
    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    // -----------------------------------------------------------------------------------
    //  Helper: setea un Dictionary privado de un CatalogoCache vía reflection
    // -----------------------------------------------------------------------------------
    private void SetDict<TK, TV>(CatalogoCache cache, string nombreCampo, Dictionary<TK, TV> contenido)
    {
        var field = typeof(CatalogoCache).GetField(nombreCampo,
            BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = (Dictionary<TK, TV>)field.GetValue(cache);
        dict.Clear();
        foreach (var kvp in contenido)
            dict[kvp.Key] = kvp.Value;
    }

    // -----------------------------------------------------------------------------------
    //  Helper: crea un CatalogoCache mock con datos de prueba
    // -----------------------------------------------------------------------------------
    private CatalogoCache CrearCatalogoCacheMock()
    {
        var go = new GameObject("CatalogoCacheMock");
        var cache = go.AddComponent<CatalogoCache>();

        SetDict(cache, "_ibermonNombres", new Dictionary<int, string>
        {
            { 25, "Pikachu" }, { 1, "Bulbasaur" }
        });
        SetDict(cache, "_ibermonNumeros", new Dictionary<string, int>
        {
            { "Pikachu", 25 }, { "Bulbasaur", 1 }
        });
        SetDict(cache, "_ibermonSpriteFrontal", new Dictionary<int, string>
        {
            { 25, "25.png" }, { 1, "1.png" }
        });
        SetDict(cache, "_ibermonSpriteTrasero", new Dictionary<int, string>
        {
            { 25, "back/25.png" }, { 1, "back/1.png" }
        });
        SetDict(cache, "_movimientoNombres", new Dictionary<int, string>
        {
            { 85, "Impactrueno" }, { 33, "Placaje" }
        });
        SetDict(cache, "_movimientoNumeros", new Dictionary<string, int>
        {
            { "Impactrueno", 85 }, { "Placaje", 33 }
        });

        return cache;
    }

    // -----------------------------------------------------------------------------------
    //  Helper: crea un PokemonBase "mínimo viable" sin pasar por el Inspector.
    //  learnableMoves debe ser una lista (vacía vale) para que Pokemon.Init() no peta.
    // -----------------------------------------------------------------------------------
    private PokemonBase CrearPokemonBaseMock(string name = "TestMon", int maxHp = 50)
    {
        var pb = ScriptableObject.CreateInstance<PokemonBase>();
        SetField(pb, "name", name);
        SetField(pb, "maxHp", maxHp);
        SetField(pb, "attack", 50);
        SetField(pb, "defense", 50);
        SetField(pb, "spAttack", 50);
        SetField(pb, "spDefense", 50);
        SetField(pb, "speed", 50);
        SetField(pb, "expYield", 100);
        SetField(pb, "catchRate", 100);
        SetField(pb, "learnableMoves", new List<LearnableMove>());
        return pb;
    }

    // -----------------------------------------------------------------------------------
    //  PR-01.1 - GetCatalogoId devuelve el número correcto
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR01_01_GetCatalogoId_DevuelveNumeroCorrecto()
    {
        var cache = CrearCatalogoCacheMock();

        int numero = cache.GetIbermonNumero("Pikachu");

        Assert.AreEqual(25, numero, "GetIbermonNumero(\"Pikachu\") debe devolver 25");

        Object.DestroyImmediate(cache.gameObject);
    }

    // -----------------------------------------------------------------------------------
    //  PR-01.2 - GetIbermonNombre con id desconocido devuelve null
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR01_02_GetIbermonNombre_IdDesconocido_DevuelveNull()
    {
        var cache = CrearCatalogoCacheMock();

        string nombre = cache.GetIbermonNombre(9999);

        Assert.IsNull(nombre,
            "Con id desconocido GetIbermonNombre debe devolver null");

        Object.DestroyImmediate(cache.gameObject);
    }

    // -----------------------------------------------------------------------------------
    //  PR-01.3 - GetMovimientoNombre / GetMovimientoNumero coherentes (round-trip)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR01_03_GetMovimiento_RoundTrip()
    {
        var cache = CrearCatalogoCacheMock();

        string nombre = cache.GetMovimientoNombre(85);
        int numero = cache.GetMovimientoNumero(nombre);

        Assert.AreEqual("Impactrueno", nombre);
        Assert.AreEqual(85, numero, "El round-trip número→nombre→número debe ser consistente");

        Object.DestroyImmediate(cache.gameObject);
    }

    // -----------------------------------------------------------------------------------
    //  PR-01.4 - ToActualizarRequest construye correctamente el payload
    //  (Usamos un PokemonBase mock con datos mínimos para que Init() no falle)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR01_04_ToActualizarRequest_PayloadCorrecto()
    {
        var cache = CrearCatalogoCacheMock();
        var pokemonBase = CrearPokemonBaseMock("TestMon", 50);

        // El constructor de Pokemon llama a Init() automáticamente.
        // Con learnableMoves = lista vacía, Init() funciona sin problemas.
        var pokemon = new Pokemon(pokemonBase, pLevel: 12);

        // Sobrescribimos los valores que queremos comprobar en el request
        pokemon.HP = 45;
        pokemon.Exp = 1200;

        var request = IbermonConverter.ToActualizarRequest(pokemon, cache);

        Assert.IsNotNull(request, "El request no debe ser null");
        Assert.AreEqual(12, request.nivel, "El nivel debe ser 12");
        Assert.AreEqual(1200, request.experiencia, "La experiencia debe ser 1200");
        Assert.AreEqual(45, request.hp_actual, "El HP_actual debe ser 45");
        Assert.IsNotNull(request.movimientos_aprendidos,
            "movimientos_aprendidos no debe ser null (aunque sea lista vacía)");

        Object.DestroyImmediate(cache.gameObject);
        Object.DestroyImmediate(pokemonBase);
    }

    // -----------------------------------------------------------------------------------
    //  PR-01.5 - ToPokemon con id desconocido devuelve null (no crash)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR01_05_ToPokemon_IbermonDesconocido_DevuelveNull()
    {
        var cache = CrearCatalogoCacheMock();
        var ibFalso = new IbermonJugador
        {
            ibermon_catalogo_id = 9999, // no existe en el cache
            nivel = 10,
            hp_actual = 30
        };

        UnityEngine.TestTools.LogAssert.Expect(LogType.Error,
            new System.Text.RegularExpressions.Regex("No se encontró el ibermon"));

        var result = IbermonConverter.ToPokemon(ibFalso, cache);

        Assert.IsNull(result, "ToPokemon con id no encontrado debe devolver null sin lanzar excepción");

        Object.DestroyImmediate(cache.gameObject);
    }
}
