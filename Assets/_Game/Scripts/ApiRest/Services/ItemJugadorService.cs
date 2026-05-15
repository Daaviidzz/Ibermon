using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

#pragma warning disable 0649

namespace ApiRest.Services
{
    // Servicio que gestiona el inventario de items del jugador
    // Todos los endpoints estan bajo /partidas/{partida_id}/items/
    public class ItemJugadorService : MonoBehaviour
    {
        // Acceso rapido al ApiManager
        private ApiManager Api => ApiManager.Instance;

        // Obtiene la lista completa de items que tiene el jugador en el inventario
        public void ObtenerInventario(string partidaId,
            Action<List<ItemJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/items/", ManejarListaInventario, onError);

            // Funcion local que envuelve el JSON y extrae la lista de items
            void ManejarListaInventario(string respuestaJson)
            {
                // Envolvemos el JSON en un objeto con campo items porque JsonUtility no lee listas sueltas
                string jsonEnvuelto = "{\"items\":" + respuestaJson + "}";
                EnvoltorioLista envoltorio = JsonUtility.FromJson<EnvoltorioLista>(jsonEnvuelto);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        // Anade un item nuevo al inventario o suma la cantidad si ya existia
        public void AnadirItem(string partidaId, int itemCatalogoId, int cantidad,
            Action<ItemJugador> onSuccess, Action<string> onError)
        {
            // Preparamos la peticion con el id del item y la cantidad que se anade
            ItemJugadorAnadirRequest peticion = new ItemJugadorAnadirRequest
            {
                item_catalogo_id = itemCatalogoId,
                cantidad = cantidad
            };

            Api.PostAuth($"/partidas/{partidaId}/items/", JsonUtility.ToJson(peticion),
                ManejarItemAnadido, onError);

            // Funcion local que deserializa el item creado y lo pasa al callback
            void ManejarItemAnadido(string respuestaJson)
            {
                ItemJugador itemNuevo = JsonUtility.FromJson<ItemJugador>(respuestaJson);
                onSuccess?.Invoke(itemNuevo);
            }
        }

        // Actualiza la cantidad de un item concreto del inventario
        public void ActualizarItem(string partidaId, string itemId, int nuevaCantidad,
            Action<ItemJugador> onSuccess, Action<string> onError)
        {
            // Preparamos la peticion con la nueva cantidad
            ItemJugadorActualizarRequest peticion = new ItemJugadorActualizarRequest
            {
                cantidad = nuevaCantidad
            };

            Api.PatchAuth($"/partidas/{partidaId}/items/{itemId}", JsonUtility.ToJson(peticion),
                ManejarItemActualizado, onError);

            // Funcion local que deserializa el item con la cantidad actualizada
            void ManejarItemActualizado(string respuestaJson)
            {
                ItemJugador itemActualizado = JsonUtility.FromJson<ItemJugador>(respuestaJson);
                onSuccess?.Invoke(itemActualizado);
            }
        }

        // Elimina un item del inventario por completo
        public void EliminarItem(string partidaId, string itemId,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/items/{itemId}", onSuccess, onError);
        }

        // Clase envoltorio privada para deserializar la lista de items
        [Serializable]
        private class EnvoltorioLista
        {
            public List<ItemJugador> items;
        }
    }
}

#pragma warning restore 0649
