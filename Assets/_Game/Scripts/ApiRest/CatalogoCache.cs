using System;
using System.Collections;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

public class CatalogoCache : MonoBehaviour
{
    public static CatalogoCache Instance { get; private set; }

    private readonly Dictionary<int,    string> _ibermonNombres    = new();
    private readonly Dictionary<string, int>    _ibermonNumeros    = new();
    private readonly Dictionary<int,    string> _movimientoNombres = new();
    private readonly Dictionary<string, int>    _movimientoNumeros = new();

    public bool EstaListo { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CargarCatalogos(Action onDone, Action<string> onError)
    {
        EstaListo = false;
        StartCoroutine(CargarCoroutine(onDone, onError));
    }

    private IEnumerator CargarCoroutine(Action onDone, Action<string> onError)
    {
        bool   ibermonListo     = false;
        bool   movimientosListo = false;
        string errorIbermon     = null;
        string errorMov         = null;

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

    public string GetIbermonNombre(int numero) =>
        _ibermonNombres.TryGetValue(numero, out var n) ? n : null;

    public int GetIbermonNumero(string nombre) =>
        _ibermonNumeros.TryGetValue(nombre, out var n) ? n : -1;

    public string GetMovimientoNombre(int numero) =>
        _movimientoNombres.TryGetValue(numero, out var n) ? n : null;

    public int GetMovimientoNumero(string nombre) =>
        _movimientoNumeros.TryGetValue(nombre, out var n) ? n : -1;
}
