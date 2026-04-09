using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    /// <summary>
    /// Endpoints:
    ///   POST   /partidas/
    ///   GET    /partidas/
    ///   GET    /partidas/{id}
    ///   PUT    /partidas/{id}/guardar
    ///   PATCH  /partidas/{id}/posicion
    ///   DELETE /partidas/{id}
    /// </summary>
    public class PartidaService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        // ------------------------------------------------------------------ //
        //  POST /partidas/
        // ------------------------------------------------------------------ //

        public void CrearPartida(string personajeElegido, int starterElegido,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            var body = new PartidaNuevaRequest
            {
                personaje_elegido = personajeElegido,
                starter_elegido = starterElegido
            };
            Api.PostAuth("/partidas/", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  GET /partidas/
        // ------------------------------------------------------------------ //

        public void ListarPartidas(Action<List<PartidaResumen>> onSuccess, Action<string> onError)
        {
            Api.GetAuth("/partidas/",
                raw =>
                {
                    // JsonUtility no deserializa listas raíz directamente
                    var wrapper = JsonUtility.FromJson<PartidaResumenListWrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(wrapper.items);
                },
                onError);
        }

        // ------------------------------------------------------------------ //
        //  GET /partidas/{id}
        // ------------------------------------------------------------------ //

        public void ObtenerPartida(string partidaId,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  PUT /partidas/{id}/guardar
        // ------------------------------------------------------------------ //

        public void GuardarPartida(string partidaId, GuardarPartidaRequest datos,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            Api.PutAuth($"/partidas/{partidaId}/guardar", JsonUtility.ToJson(datos),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  PATCH /partidas/{id}/posicion
        // ------------------------------------------------------------------ //

        public void ActualizarPosicion(string partidaId, string mapaActual, float x, float y,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            var body = new ActualizarPosicionRequest
            {
                mapa_actual = mapaActual,
                posicion = new Posicion { x = x, y = y }
            };
            Api.PatchAuth($"/partidas/{partidaId}/posicion", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  DELETE /partidas/{id}
        // ------------------------------------------------------------------ //

        public void EliminarPartida(string partidaId, Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}", onSuccess, onError);
        }

        // ------------------------------------------------------------------ //
        //  Wrapper interno para lista
        // ------------------------------------------------------------------ //

        [Serializable]
        private class PartidaResumenListWrapper { public List<PartidaResumen> items; }
    }
}
