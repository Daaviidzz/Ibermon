using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    // Endpoints bajo /partidas/{partida_id}/ibermon/
    public class IbermonJugadorService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        // Los ibermon del equipo activo
        public void ObtenerEquipo(string partidaId,
            Action<List<IbermonJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/ibermon/equipo",
                raw => onSuccess?.Invoke(ParseList(raw)),
                onError);
        }

        // Los ibermon guardados en el centro — no se usan en combate
        public void ObtenerCentro(string partidaId,
            Action<List<IbermonJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/ibermon/centro",
                raw => onSuccess?.Invoke(ParseList(raw)),
                onError);
        }

        // Capturar un ibermon nuevo
        public void AnadirIbermon(string partidaId, IbermonJugadorCrearRequest datos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PostAuth($"/partidas/{partidaId}/ibermon/", JsonUtility.ToJson(datos),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // Mover un ibermon entre equipo y centro
        public void MoverIbermon(string partidaId, string ibermonId, string ubicacion,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            var body = new IbermonJugadorMoverRequest { ubicacion = ubicacion };
            Api.PatchAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/mover", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        // Actualizar stats después de un combate (HP, nivel, exp, movimientos)
        public void ActualizarIbermon(string partidaId, string ibermonId, IbermonJugadorActualizarRequest datos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PatchAuth($"/partidas/{partidaId}/ibermon/{ibermonId}", JsonUtility.ToJson(datos),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        public void EliminarIbermon(string partidaId, string ibermonId,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/ibermon/{ibermonId}", onSuccess, onError);
        }

        // Reemplaza la lista completa de movimientos de un ibermon
        public void ActualizarMovimientos(string partidaId, string ibermonId, List<MovimientoAprendido> movimientos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            // JsonUtility no serializa listas sueltas, hay que construirlo a mano
            var sb = new System.Text.StringBuilder("[");
            for (int i = 0; i < movimientos.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append($"{{\"numero\":{movimientos[i].numero},\"pp\":{movimientos[i].pp}}}");
            }
            sb.Append("]");
            Api.PutAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos", sb.ToString(),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        public void AprenderMovimiento(string partidaId, string ibermonId, int numeroMovimiento,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PostAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos/{numeroMovimiento}", "{}",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<IbermonJugador>(raw)),
                onError);
        }

        public void OlvidarMovimiento(string partidaId, string ibermonId, int numeroMovimiento,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos/{numeroMovimiento}",
                onSuccess, onError);
        }

        private List<IbermonJugador> ParseList(string raw)
        {
            var w = JsonUtility.FromJson<Wrapper>("{\"items\":" + raw + "}");
            return w.items;
        }

        [Serializable]
        private class Wrapper { public List<IbermonJugador> items; }
    }
}
