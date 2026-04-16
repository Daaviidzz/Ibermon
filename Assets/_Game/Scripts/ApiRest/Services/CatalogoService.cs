using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    public class CatalogoService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;


        public void ListarIbermon(Action<List<IbermonCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/ibermon",
                raw =>
                {
                    var w = JsonUtility.FromJson<IbermonResumenWrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(w.items);
                },
                onError);
        }

        public void ObtenerIbermon(int numero,
            Action<IbermonCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/ibermon/{numero}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonCatalogoDetalle>(raw)),
                onError);
        }



        public void ListarMovimientos(Action<List<MovimientoCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/movimientos",
                raw =>
                {
                    var w = JsonUtility.FromJson<MovimientoResumenWrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(w.items);
                },
                onError);
        }

        public void ObtenerMovimiento(int numero,
            Action<MovimientoCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/movimientos/{numero}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<MovimientoCatalogoDetalle>(raw)),
                onError);
        }


        public void ListarItems(Action<List<ItemCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/items",
                raw =>
                {
                    var w = JsonUtility.FromJson<ItemResumenWrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(w.items);
                },
                onError);
        }

        public void ObtenerItem(int numero,
            Action<ItemCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/items/{numero}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<ItemCatalogoDetalle>(raw)),
                onError);
        }



        public void ListarLogros(Action<List<LogroCatalogo>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/logros",
                raw =>
                {
                    var w = JsonUtility.FromJson<LogroWrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(w.items);
                },
                onError);
        }

        public void ObtenerLogro(string codigo,
            Action<LogroCatalogo> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/logros/{codigo}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<LogroCatalogo>(raw)),
                onError);
        }


        [Serializable] private class IbermonResumenWrapper { public List<IbermonCatalogoResumen> items; }
        [Serializable] private class MovimientoResumenWrapper { public List<MovimientoCatalogoResumen> items; }
        [Serializable] private class ItemResumenWrapper { public List<ItemCatalogoResumen> items; }
        [Serializable] private class LogroWrapper { public List<LogroCatalogo> items; }
    }
}
