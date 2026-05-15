using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

#pragma warning disable 0649

namespace ApiRest.Services
{
    // Servicio que consulta los catalogos del juego
    // Los endpoints del catalogo son publicos y no necesitan autenticacion
    // Endpoints:
    //   GET /catalogo/ibermon
    //   GET /catalogo/ibermon/{numero}
    //   GET /catalogo/movimientos
    //   GET /catalogo/movimientos/{numero}
    //   GET /catalogo/items
    //   GET /catalogo/items/{numero}
    //   GET /catalogo/logros
    //   GET /catalogo/logros/{codigo}
    public class CatalogoService : MonoBehaviour
    {
        // Acceso rapido al ApiManager
        private ApiManager Api => ApiManager.Instance;

        // Pide al servidor la lista de todos los ibermon del catalogo en version resumida
        public void ListarIbermon(Action<List<IbermonCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/ibermon", ManejarListaIbermon, onError);

            // Funcion local que envuelve el JSON y extrae la lista
            void ManejarListaIbermon(string respuestaJson)
            {
                IbermonResumenWrapper envoltorio = EnvolverYDeserializar<IbermonResumenWrapper>(respuestaJson);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        // Pide al servidor los datos completos de un ibermon concreto por su numero
        public void ObtenerIbermon(int numero,
            Action<IbermonCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/ibermon/{numero}", ManejarDetalleIbermon, onError);

            // Funcion local que deserializa el detalle y lo pasa al callback
            void ManejarDetalleIbermon(string respuestaJson)
            {
                IbermonCatalogoDetalle detalle = JsonUtility.FromJson<IbermonCatalogoDetalle>(respuestaJson);
                onSuccess?.Invoke(detalle);
            }
        }

        // Pide al servidor la lista de todos los movimientos del catalogo en version resumida
        public void ListarMovimientos(Action<List<MovimientoCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/movimientos", ManejarListaMovimientos, onError);

            // Funcion local que envuelve el JSON y extrae la lista
            void ManejarListaMovimientos(string respuestaJson)
            {
                MovimientoResumenWrapper envoltorio = EnvolverYDeserializar<MovimientoResumenWrapper>(respuestaJson);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        // Pide al servidor los datos completos de un movimiento concreto por su numero
        public void ObtenerMovimiento(int numero,
            Action<MovimientoCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/movimientos/{numero}", ManejarDetalleMovimiento, onError);

            // Funcion local que deserializa el detalle y lo pasa al callback
            void ManejarDetalleMovimiento(string respuestaJson)
            {
                MovimientoCatalogoDetalle detalle = JsonUtility.FromJson<MovimientoCatalogoDetalle>(respuestaJson);
                onSuccess?.Invoke(detalle);
            }
        }

        // Pide al servidor la lista de todos los items del catalogo en version resumida
        public void ListarItems(Action<List<ItemCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/items", ManejarListaItems, onError);

            // Funcion local que envuelve el JSON y extrae la lista
            void ManejarListaItems(string respuestaJson)
            {
                ItemResumenWrapper envoltorio = EnvolverYDeserializar<ItemResumenWrapper>(respuestaJson);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        // Pide al servidor los datos completos de un item concreto por su numero
        public void ObtenerItem(int numero,
            Action<ItemCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/items/{numero}", ManejarDetalleItem, onError);

            // Funcion local que deserializa el detalle y lo pasa al callback
            void ManejarDetalleItem(string respuestaJson)
            {
                ItemCatalogoDetalle detalle = JsonUtility.FromJson<ItemCatalogoDetalle>(respuestaJson);
                onSuccess?.Invoke(detalle);
            }
        }

        // Pide al servidor la lista de todos los logros del catalogo
        public void ListarLogros(Action<List<LogroCatalogo>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/logros", ManejarListaLogros, onError);

            // Funcion local que envuelve el JSON y extrae la lista
            void ManejarListaLogros(string respuestaJson)
            {
                LogroWrapper envoltorio = EnvolverYDeserializar<LogroWrapper>(respuestaJson);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        // Pide al servidor los datos de un logro concreto por su codigo
        public void ObtenerLogro(string codigo,
            Action<LogroCatalogo> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/logros/{codigo}", ManejarDetalleLogro, onError);

            // Funcion local que deserializa el logro y lo pasa al callback
            void ManejarDetalleLogro(string respuestaJson)
            {
                LogroCatalogo logro = JsonUtility.FromJson<LogroCatalogo>(respuestaJson);
                onSuccess?.Invoke(logro);
            }
        }

        // Metodo auxiliar para envolver una lista de JSON dentro de un objeto con campo items
        // JsonUtility de Unity no puede deserializar una lista en la raiz del JSON
        // Por eso envolvemos el JSON en {"items": ...} antes de deserializarlo
        private TEnvoltorio EnvolverYDeserializar<TEnvoltorio>(string jsonLista)
        {
            string jsonEnvuelto = "{\"items\":" + jsonLista + "}";
            return JsonUtility.FromJson<TEnvoltorio>(jsonEnvuelto);
        }

        // Clases envoltorio privadas para deserializar las listas del catalogo
        [Serializable] private class IbermonResumenWrapper { public List<IbermonCatalogoResumen> items; }
        [Serializable] private class MovimientoResumenWrapper { public List<MovimientoCatalogoResumen> items; }
        [Serializable] private class ItemResumenWrapper { public List<ItemCatalogoResumen> items; }
        [Serializable] private class LogroWrapper { public List<LogroCatalogo> items; }
    }
}

#pragma warning restore 0649
