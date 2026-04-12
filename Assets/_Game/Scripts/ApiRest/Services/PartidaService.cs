using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    // Endpoints:
    //   POST   /partidas/
    //   GET    /partidas/
    //   GET    /partidas/{id}
    //   PUT    /partidas/{id}/guardar
    //   PATCH  /partidas/{id}/posicion
    //   DELETE /partidas/{id}
    public class PartidaService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        public void CrearPartida(string personajeElegido, int starterElegido,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            var body = new PartidaNuevaRequest
            {
                personaje_elegido = personajeElegido,
                starter_elegido   = starterElegido
            };
            Api.PostAuth("/partidas/", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        public void ListarPartidas(Action<List<PartidaResumen>> onSuccess, Action<string> onError)
        {
            Api.GetAuth("/partidas/",
                raw =>
                {
                    // JsonUtility no deserializa listas en la raíz directamente, hay que envolverlas
                    var wrapper = JsonUtility.FromJson<PartidaResumenListWrapper>("{\"items\":" + raw + "}");
                    onSuccess?.Invoke(wrapper.items);
                },
                onError);
        }

        public void ObtenerPartida(string partidaId,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        public void GuardarPartida(string partidaId, GuardarPartidaRequest datos,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            Api.PutAuth($"/partidas/{partidaId}/guardar", JsonUtility.ToJson(datos),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        public void ActualizarPosicion(string partidaId, string mapaActual, float x, float y,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            var body = new ActualizarPosicionRequest
            {
                mapa_actual = mapaActual,
                posicion    = new Posicion { x = x, y = y }
            };
            Api.PatchAuth($"/partidas/{partidaId}/posicion", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<PartidaCompleta>(raw)),
                onError);
        }

        public void EliminarPartida(string partidaId, Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}", onSuccess, onError);
        }

        [Serializable]
        private class PartidaResumenListWrapper { public List<PartidaResumen> items; }
    }
}
