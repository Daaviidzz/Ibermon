using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-16 — Tests para TorrenteFrases (Bloque 3 - Jugador)
//
//  TorrenteFrases gestiona la reproducción aleatoria de frases ambientales del
//  protagonista cada 5 minutos. Los tests más importantes son:
//   · ultimoIndice se inicializa a -1
//   · ReproducirFraseRandom no repite la última frase consecutivamente
//   · No reproduce nada si la lista de clips está vacía (no crash)
//
//  No podemos validar AudioSource.PlayOneShot directamente sin un AudioListener
//  activo, pero sí podemos comprobar que ultimoIndice avanza correctamente.
// =====================================================================================
public class TestTorrenteFrases
{
    private TorrenteFrases _frases;
    private GameObject _go;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("TorrenteFrasesTest");
        // SetActive(false) para evitar que Start() ejecute InvokeRepeating
        _go.SetActive(false);
        _frases = _go.AddComponent<TorrenteFrases>();
        _frases.audioSource = _go.AddComponent<AudioSource>();

        // Creamos 3 AudioClips falsos (instancias vacías son suficientes)
        _frases.frasesTorrente = new List<AudioClip>
        {
            AudioClip.Create("Clip0", 44100, 1, 44100, false),
            AudioClip.Create("Clip1", 44100, 1, 44100, false),
            AudioClip.Create("Clip2", 44100, 1, 44100, false)
        };
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  Helper: lee el campo privado ultimoIndice
    // -----------------------------------------------------------------------------------
    private int GetUltimoIndice()
    {
        var field = typeof(TorrenteFrases).GetField("ultimoIndice",
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (int)field.GetValue(_frases);
    }

    private void SetUltimoIndice(int valor)
    {
        var field = typeof(TorrenteFrases).GetField("ultimoIndice",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_frases, valor);
    }

    private void InvocarReproducirFraseRandom()
    {
        var metodo = typeof(TorrenteFrases).GetMethod("ReproducirFraseRandom",
            BindingFlags.NonPublic | BindingFlags.Instance);
        metodo.Invoke(_frases, null);
    }

    // -----------------------------------------------------------------------------------
    //  PR-16.1 - ultimoIndice inicial es -1 (antes de reproducir ninguna)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR16_01_UltimoIndice_InicialMenosUno()
    {
        Assert.AreEqual(-1, GetUltimoIndice(),
            "ultimoIndice debe ser -1 antes de la primera reproducción");
    }

    // -----------------------------------------------------------------------------------
    //  PR-16.2 - Tras llamar ReproducirFraseRandom, ultimoIndice ya no es -1
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR16_02_ReproducirFraseRandom_ActualizaUltimoIndice()
    {
        // Forzamos Awake manualmente (porque SetActive(false))
        _frases.gameObject.SetActive(true);

        InvocarReproducirFraseRandom();

        int indice = GetUltimoIndice();
        Assert.IsTrue(indice >= 0 && indice < _frases.frasesTorrente.Count,
            $"ultimoIndice tras reproducir debe estar en rango [0, {_frases.frasesTorrente.Count - 1}], era {indice}");
    }

    // -----------------------------------------------------------------------------------
    //  PR-16.3 - Si Random elige el mismo índice que el último, se salta al siguiente
    //  Esto evita escuchar la misma frase dos veces seguidas.
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR16_03_NoRepiteFraseConsecutiva()
    {
        _frases.gameObject.SetActive(true);

        // Forzamos Random.seed para que la primera llamada elija un índice determinista,
        // luego invocamos varias veces y comprobamos que el índice cambia.
        Random.InitState(1);

        int previo = -1;
        for (int i = 0; i < 10; i++)
        {
            InvocarReproducirFraseRandom();
            int actual = GetUltimoIndice();
            if (previo >= 0)
            {
                Assert.AreNotEqual(previo, actual,
                    $"ultimoIndice debe cambiar tras cada llamada (iteración {i})");
            }
            previo = actual;
        }
    }

    // -----------------------------------------------------------------------------------
    //  PR-16.4 - Lista vacía no lanza excepción (early return)
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR16_04_ListaVacia_NoLanzaExcepcion()
    {
        _frases.frasesTorrente.Clear();
        _frases.gameObject.SetActive(true);

        Assert.DoesNotThrow(() => InvocarReproducirFraseRandom(),
            "ReproducirFraseRandom con lista vacía debe hacer early return sin excepción");
    }
}
