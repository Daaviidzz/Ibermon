using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    /// <summary>
    /// Endpoints bajo /partidas/{partida_id}/ibermon/
    /// </summary>
    public class IbermonJugadorService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        // ------------------------------------------------------------------ //
        //  GET /partidas/{id}/ibermon/equipo
        // ------------------------------------------------------------------ //

        public void ObtenerEquipo(string partidaId,
            Action<List<IbermonJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/ibermon/equipo",
                raw => onSuccess?.Invoke(ParseList(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  GET /partidas/{id}/ibermon/centro
        // ------------------------------------------------------------------ //

        public void ObtenerCentro(string partidaId,
            Action<List<IbermonJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/ibermon/centro",
                raw => onSuccess?.Invoke(ParseList(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  POST /partidas/{id}/ibermon/
        // ------------------------------------------------------------------ //

        public void AnadirIbermon(string partidaId, IbermonJugadorCrearRequest datos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PostAuth($"/partidas/{partidaId}/ibermon/", JsonUtility.ToJson(datos),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  PATCH /partidas/{id}/ibermon/{ibermon_id}/mover
        // ------------------------------------------------------------------ //

        public void MoverIbermon(string partidaId, string ibermonId, string ubicacion,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            var body = new IbermonJugadorMoverRequest { ubicacion = ubicacion };
            Api.PatchAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/mover", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  PATCH /partidas/{id}/ibermon/{ibermon_id}
        // ------------------------------------------------------------------ //

        public void ActualizarIbermon(string partidaId, string ibermonId, IbermonJugadorActualizarRequest datos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PatchAuth($"/partidas/{partidaId}/ibermon/{ibermonId}", JsonUtility.ToJson(datos),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  DELETE /partidas/{id}/ibermon/{ibermon_id}
        // ------------------------------------------------------------------ //

        public void EliminarIbermon(string partidaId, string ibermonId,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/ibermon/{ibermonId}", onSuccess, onError);
        }

        // ------------------------------------------------------------------ //
        //  PUT /partidas/{id}/ibermon/{ibermon_id}/movimientos
        // ------------------------------------------------------------------ //

        public void ActualizarMovimientos(string partidaId, string ibermonId, List<int> movimientos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            // La API espera un JSON array directamente
            string json = "[" + string.Join(",", movimientos) + "]";
            Api.PutAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos", json,
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  POST /partidas/{id}/ibermon/{ibermon_id}/movimientos/{numero}
        // ------------------------------------------------------------------ //

        public void AprenderMovimiento(string partidaId, string ibermonId, int numeroMovimiento,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PostAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos/{numeroMovimiento}", "{}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  DELETE /partidas/{id}/ibermon/{ibermon_id}/movimientos/{numero}
        // ------------------------------------------------------------------ //

        public void OlvidarMovimiento(string partidaId, string ibermonId, int numeroMovimiento,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos/{numeroMovimiento}",
                onSuccess, onError);
        }

        // ------------------------------------------------------------------ //
        //  Helper
        // ------------------------------------------------------------------ //

        private List<IbermonJugador> ParseList(string raw)
        {
            var w = JsonUtility.FromJson<Wrapper>("{\"items\":" + raw + "}");
            return w.items;
        }

        [Serializable]
        private class Wrapper { public List<IbermonJugador> items; }
    }
}
