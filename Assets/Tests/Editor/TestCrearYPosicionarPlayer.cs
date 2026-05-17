using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-15 — Tests para CrearYPosicionarPlayer (Bloque 3 - Transiciones)
//
//  CrearYPosicionarPlayer es el componente que instancia el prefab del personaje
//  correcto según el campo "personaje_elegido" que devuelve la API. Tiene un
//  diccionario interno {"torrente":0, "personaje1":1, ...} que mapea el nombre
//  al índice de la lista de prefabs.
//
//  ObtenerPrefabPersonaje es privado, así que usamos reflection. Para los
//  prefabs no podemos usar instancias reales (vienen del proyecto), pero
//  podemos usar GameObjects vacíos como marcadores.
// =====================================================================================
public class TestCrearYPosicionarPlayer
{
    private CrearYPosicionarPlayer _spawner;
    private GameObject _go;
    private GameObject _prefabTorrente;
    private GameObject _prefabPersonaje1;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("SpawnerTest");
        _spawner = _go.AddComponent<CrearYPosicionarPlayer>();

        // Creamos GameObjects que harán de "prefabs" para el test
        _prefabTorrente = new GameObject("PrefabTorrente");
        _prefabPersonaje1 = new GameObject("PrefabPersonaje1");

        _spawner.personajes = new List<GameObject> { _prefabTorrente, _prefabPersonaje1 };

        // Reset de JugadorSpawn entre tests
        JugadorSpawn.usarPosicionGuardada = false;
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
        if (_prefabTorrente != null) Object.DestroyImmediate(_prefabTorrente);
        if (_prefabPersonaje1 != null) Object.DestroyImmediate(_prefabPersonaje1);
    }

    // -----------------------------------------------------------------------------------
    //  Helper: llama al método privado ObtenerPrefabPersonaje vía reflection
    // -----------------------------------------------------------------------------------
    private GameObject InvokarObtenerPrefab(string identificador)
    {
        var metodo = typeof(CrearYPosicionarPlayer).GetMethod("ObtenerPrefabPersonaje",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (GameObject)metodo.Invoke(_spawner, new object[] { identificador });
    }

    // -----------------------------------------------------------------------------------
    //  PR-15.1 - "torrente" devuelve el prefab de índice 0
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR15_01_Torrente_DevuelvePrimerPrefab()
    {
        var prefab = InvokarObtenerPrefab("torrente");
        Assert.AreSame(_prefabTorrente, prefab,
            "El identificador 'torrente' debe devolver el primer prefab de la lista");
    }

    // -----------------------------------------------------------------------------------
    //  PR-15.2 - "personaje1" devuelve el prefab de índice 1
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR15_02_Personaje1_DevuelveSegundoPrefab()
    {
        var prefab = InvokarObtenerPrefab("personaje1");
        Assert.AreSame(_prefabPersonaje1, prefab,
            "El identificador 'personaje1' debe devolver el segundo prefab");
    }

    // -----------------------------------------------------------------------------------
    //  PR-15.3 - Identificador desconocido cae al primer prefab (fallback) con LogWarning
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR15_03_Desconocido_FallbackAlPrimero()
    {
        UnityEngine.TestTools.LogAssert.Expect(LogType.Warning,
            new System.Text.RegularExpressions.Regex("no reconocido"));

        var prefab = InvokarObtenerPrefab("personajeQueNoExiste");
        Assert.AreSame(_prefabTorrente, prefab,
            "Un identificador desconocido debe caer al primer prefab de la lista (fallback)");
    }

    // -----------------------------------------------------------------------------------
    //  PR-15.4 - Comparación de strings es case-insensitive
    //  (El diccionario usa StringComparer.OrdinalIgnoreCase)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR15_04_Identificador_CaseInsensitive()
    {
        var prefabA = InvokarObtenerPrefab("Torrente");   // mayúscula inicial
        var prefabB = InvokarObtenerPrefab("TORRENTE");   // todo mayúsculas
        var prefabC = InvokarObtenerPrefab("torrente");   // minúsculas

        Assert.AreSame(_prefabTorrente, prefabA);
        Assert.AreSame(_prefabTorrente, prefabB);
        Assert.AreSame(_prefabTorrente, prefabC);
    }
}
