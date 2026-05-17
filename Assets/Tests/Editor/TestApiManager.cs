using NUnit.Framework;
using ApiRest.Managers;
using UnityEngine;

// =====================================================================================
//  PR-04 — Tests para ApiManager (Bloque 1 - API REST)
//
//  ApiManager es el wrapper de UnityWebRequest que centraliza todas las peticiones
//  HTTP. Mantiene el token JWT y lo añade automáticamente a cada petición.
//
//  Como las peticiones reales requieren un servidor levantado, aquí probamos solo
//  la parte sincronizada y observable: gestión del singleton, almacenamiento del
//  token, y configuración inicial.
// =====================================================================================
public class TestApiManager
{
    private GameObject _go;
    private ApiManager _api;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("ApiManagerTest");
        _api = _go.AddComponent<ApiManager>();
    }

    [TearDown]
    public void TearDown()
    {
        // Reset del singleton al final de cada test
        var field = typeof(ApiManager).GetProperty("Instance",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        // Instance es un property con get privado y set privado en este caso
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  PR-04.1 - Estado inicial: IsLoggedIn = false
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR04_01_EstadoInicial_NoLoggeado()
    {
        Assert.IsFalse(_api.IsLoggedIn,
            "Recién creado, ApiManager NO debe estar logueado (sin token)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-04.2 - SetToken marca como logueado y ClearToken lo revierte
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR04_02_SetToken_CambiaIsLoggedIn()
    {
        _api.SetToken("token_jwt_de_prueba_aBcDeFg123");
        Assert.IsTrue(_api.IsLoggedIn,
            "Tras SetToken con un token no vacío, IsLoggedIn debe ser true");

        _api.ClearToken();
        Assert.IsFalse(_api.IsLoggedIn,
            "Tras ClearToken, IsLoggedIn debe volver a false");
    }

    // -----------------------------------------------------------------------------------
    //  PR-04.3 - SetToken con string vacío deja IsLoggedIn = false
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR04_03_SetToken_Vacio_NoLogueado()
    {
        _api.SetToken("");
        Assert.IsFalse(_api.IsLoggedIn,
            "Token vacío no cuenta como sesión válida");

        _api.SetToken(null);
        Assert.IsFalse(_api.IsLoggedIn,
            "Token null tampoco cuenta como sesión válida");
    }

    // -----------------------------------------------------------------------------------
    //  PR-04.4 - BaseUrl no debe ser null (tiene valor por defecto)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR04_04_BaseUrl_TieneValorPorDefecto()
    {
        Assert.IsNotNull(_api.BaseUrl, "BaseUrl nunca debe ser null");
        Assert.IsTrue(_api.BaseUrl.StartsWith("http"),
            "BaseUrl debe empezar por http o https");
    }
}
