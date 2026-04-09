using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    /// <summary>
    /// Endpoints bajo /partidas/{partida_id}/items/
    /// </summary>
    public class ItemJugadorService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        // ------------------------------------------------------------------ //
        //  GET /partidas/{id}/items/
        // ------------------------------------------------------------------ //

        public void ObtenerInventario(string partidaId,
            Action<List<ItemJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/items/",
                raw =>
                {
                    var w = JsonUtility.FromJson<Wrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(w.items);
                },
                onError);
        }

        // ------------------------------------------------------------------ //
        //  POST /partidas/{id}/items/
        // ------------------------------------------------------------------ //

        public void AnadirItem(string partidaId, int itemCatalogoId, int cantidad,
            Action<ItemJugador> onSuccess, Action<string> onError)
        {
            var body = new ItemJugadorAnadirRequest
            {
                item_catalogo_id = itemCatalogoId,
                cantidad = cantidad
            };
            Api.PostAuth($"/partidas/{partidaId}/items/", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<ItemJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  PATCH /partidas/{id}/items/{item_id}
        // ------------------------------------------------------------------ //

        public void ActualizarItem(string partidaId, string itemId, int nuevaCantidad,
            Action<ItemJugador> onSuccess, Action<string> onError)
        {
            var body = new ItemJugadorActualizarRequest { cantidad = nuevaCantidad };
            Api.PatchAuth($"/partidas/{partidaId}/items/{itemId}", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<ItemJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  DELETE /partidas/{id}/items/{item_id}
        // ------------------------------------------------------------------ //

        public void EliminarItem(string partidaId, string itemId,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/items/{itemId}", onSuccess, onError);
        }

        [Serializable]
        private class Wrapper { public List<ItemJugador> items; }
    }
}
