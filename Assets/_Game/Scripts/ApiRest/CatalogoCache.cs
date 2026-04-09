using System;
using System.Collections;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

/// <summary>
/// Singleton DontDestroyOnLoad.
/// Pre-carga los catálogos de ibermon y movimientos desde la API y los mantiene
/// en memoria para conversiones rápidas numero↔nombre.
/// Se crea automáticamente por ApiSetup. No añadir manualmente a la escena.
///
/// Uso: después del login, llama CargarCatalogos(onDone, onError) una vez.
/// A partir de ahí usa los getters directamente.
/// </summary>
public class CatalogoCache : MonoBehaviour
{
    public static CatalogoCache Instance { get; private set; }

    // ─── Mapas numero ↔ nombre ────────────────────────────────────────────────

    private readonly Dictionary<int,    string> _ibermonNombres    = new();
    private readonly Dictionary<string, int>    _ibermonNumeros    = new();
    private readonly Dictionary<int,    string> _movimientoNombres = new();
    private readonly Dictionary<string, int>    _movimientoNumeros = new();

    /// <summary>True cuando los catálogos han sido cargados correctamente.</summary>
    public bool EstaListo { get; private set; } = false;

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Carga ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Carga los catálogos de ibermon y movimientos desde la API.
    /// Llama onDone cuando ambos estén listos, o onError si alguno falla.
    /// Las peticiones son públicas (no requieren JWT).
    /// </summary>
    public void CargarCatalogos(Action onDone, Action<string> onError)
    {
        EstaListo = false;
        StartCoroutine(CargarCoroutine(onDone, onError));
    }

    private IEnumerator CargarCoroutine(Action onDone, Action<string> onError)
    {
        bool ibermonListo      = false;
        bool movimientosListo  = false;
        string errorIbermon    = null;
        string errorMov        = null;

        // ── Ibermon ──────────────────────────────────────────────────────────
        ApiSetup.Catalogo.ListarIbermon(
            lista =>
            {
                _ibermonNombres.Clear();
                _ibermonNumeros.Clear();
                foreach (var ib in lista)
                {
                    _ibermonNombres[ib.numero] = ib.nombre;
                    _ibermonNumeros[ib.nombre] = ib.numero;
                }
                Debug.Log($"[CatalogoCache] Ibermon cargados: {lista.Count}");
                ibermonListo = true;
            },
            err => { errorIbermon = err; ibermonListo = true; }
        );

        // ── Movimientos ───────────────────────────────────────────────────────
        ApiSetup.Catalogo.ListarMovimientos(
            lista =>
            {
                _movimientoNombres.Clear();
                _movimientoNumeros.Clear();
                foreach (var mov in lista)
                {
                    _movimientoNombres[mov.numero] = mov.nombre;
                    _movimientoNumeros[mov.nombre] = mov.numero;
                }
                Debug.Log($"[CatalogoCache] Movimientos cargados: {lista.Count}");
                movimientosListo = true;
            },
            err => { errorMov = err; movimientosListo = true; }
        );

        yield return new WaitUntil(() => ibermonListo && movimientosListo);

        if (errorIbermon != null || errorMov != null)
        {
            string msg = errorIbermon ?? errorMov;
            Debug.LogError($"[CatalogoCache] Error cargando catálogos: {msg}");
            onError?.Invoke(msg);
            yield break;
        }

        EstaListo = true;
        onDone?.Invoke();
    }

    // ─── Lookups ibermon ──────────────────────────────────────────────────────

    /// <summary>Devuelve el nombre del ibermon dado su número de catálogo. Null si no existe.</summary>
    public string GetIbermonNombre(int numero) =>
        _ibermonNombres.TryGetValue(numero, out var n) ? n : null;

    /// <summary>Devuelve el número de catálogo del ibermon dado su nombre. -1 si no existe.</summary>
    public int GetIbermonNumero(string nombre) =>
        _ibermonNumeros.TryGetValue(nombre, out var n) ? n : -1;

    // ─── Lookups movimientos ──────────────────────────────────────────────────

    /// <summary>Devuelve el nombre del movimiento dado su número. Null si no existe.</summary>
    public string GetMovimientoNombre(int numero) =>
        _movimientoNombres.TryGetValue(numero, out var n) ? n : null;

    /// <summary>Devuelve el número del movimiento dado su nombre. -1 si no existe.</summary>
    public int GetMovimientoNumero(string nombre) =>
        _movimientoNumeros.TryGetValue(nombre, out var n) ? n : -1;
}
