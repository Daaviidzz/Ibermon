using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ApiRest.Models;
using UnityEngine;

// =====================================================================================
//  PR-02 — Tests para CatalogoCache (Bloque 1 - API REST)  [v2 corregido]
//
//  CatalogoCache es la clase que mantiene en memoria los catálogos descargados
//  desde la API (ibermon, movimientos) y ofrece métodos de búsqueda rápida.
//  Es crítico para el conversor y para todo el sistema de combate.
//
//  La clase usa internamente seis Dictionary distintos (no listas) para los
//  diferentes lookups: nombres por número, números por nombre, paths de sprite,
//  movimientos por número y movimientos por nombre. Aquí los inyectamos por
//  reflection con datos de prueba conocidos.
// =====================================================================================
public class TestCatalogoCache
{
    private CatalogoCache _cache;
    private GameObject _go;

    // -----------------------------------------------------------------------------------
    //  Helper: setea un Dictionary privado de CatalogoCache vía reflection
    // -----------------------------------------------------------------------------------
    private void SetDict<TK, TV>(string nombreCampo, Dictionary<TK, TV> contenido)
    {
        var field = typeof(CatalogoCache).GetField(nombreCampo,
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, $"No se encontró el campo privado {nombreCampo} en CatalogoCache");

        var dict = (Dictionary<TK, TV>)field.GetValue(_cache);
        dict.Clear();
        foreach (var kvp in contenido)
            dict[kvp.Key] = kvp.Value;
    }

    // -----------------------------------------------------------------------------------
    //  Setup: antes de cada test, instanciamos un CatalogoCache "limpio" con datos
    //  conocidos en sus 6 diccionarios internos.
    // -----------------------------------------------------------------------------------
    [SetUp]
    public void Setup()
    {
        _go = new GameObject("CatalogoCacheTest");
        _cache = _go.AddComponent<CatalogoCache>();

        SetDict("_ibermonNombres", new Dictionary<int, string>
        {
            { 1,  "Bulbasaur" },
            { 4,  "Charmander" },
            { 25, "Pikachu" }
        });
        SetDict("_ibermonNumeros", new Dictionary<string, int>
        {
            { "Bulbasaur",  1 },
            { "Charmander", 4 },
            { "Pikachu",    25 }
        });
        SetDict("_ibermonSpriteFrontal", new Dictionary<int, string>
        {
            { 1, "1.png" }, { 4, "4.png" }, { 25, "25.png" }
        });
        SetDict("_ibermonSpriteTrasero", new Dictionary<int, string>
        {
            { 1, "back/1.png" }, { 4, "back/4.png" }, { 25, "back/25.png" }
        });
        SetDict("_movimientoNombres", new Dictionary<int, string>
        {
            { 33, "Placaje" }, { 85, "Impactrueno" }
        });
        SetDict("_movimientoNumeros", new Dictionary<string, int>
        {
            { "Placaje", 33 }, { "Impactrueno", 85 }
        });
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  PR-02.1 - GetIbermonNombre por id existente
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR02_01_GetIbermonNombre_IdExistente_OK()
    {
        Assert.AreEqual("Bulbasaur", _cache.GetIbermonNombre(1));
        Assert.AreEqual("Pikachu", _cache.GetIbermonNombre(25));
    }

    // -----------------------------------------------------------------------------------
    //  PR-02.2 - GetIbermonNumero por nombre devuelve número correcto
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR02_02_GetIbermonNumero_NombreExistente_OK()
    {
        Assert.AreEqual(4, _cache.GetIbermonNumero("Charmander"));
        Assert.AreEqual(25, _cache.GetIbermonNumero("Pikachu"));
    }

    // -----------------------------------------------------------------------------------
    //  PR-02.3 - GetSpriteFrontal y GetSpriteTrasero devuelven los paths esperados
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR02_03_GetSprite_PathsCorrectos()
    {
        Assert.AreEqual("1.png", _cache.GetSpriteFrontal(1));
        Assert.AreEqual("back/1.png", _cache.GetSpriteTrasero(1));
        Assert.AreEqual("25.png", _cache.GetSpriteFrontal(25));
    }

    // -----------------------------------------------------------------------------------
    //  PR-02.4 - Lookup con id/nombre no existente devuelve null o -1
    //  (CatalogoCache devuelve -1 para números, null para strings)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR02_04_Lookup_NoExistente_RetornaVacioOMinusUno()
    {
        Assert.IsNull(_cache.GetIbermonNombre(9999),
            "Id desconocido debe devolver null");
        Assert.AreEqual(-1, _cache.GetIbermonNumero("MonstruoQueNoExiste"),
            "Nombre desconocido debe devolver -1 (no 0)");
        Assert.IsNull(_cache.GetMovimientoNombre(9999),
            "Movimiento desconocido debe devolver null");
        Assert.AreEqual(-1, _cache.GetMovimientoNumero("MovimientoQueNoExiste"),
            "Movimiento desconocido por nombre debe devolver -1");
    }
}
