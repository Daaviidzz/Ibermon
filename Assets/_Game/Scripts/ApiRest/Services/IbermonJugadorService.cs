using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

#pragma warning disable 0649

namespace ApiRest.Services
{
    // Servicio que gestiona los ibermon del jugador dentro de una partida
    // Todos los endpoints estan bajo /partidas/{partida_id}/ibermon/
    public class IbermonJugadorService : MonoBehaviour
    {
        // Acceso rapido al ApiManager
        private ApiManager Api => ApiManager.Instance;

        // Obtiene la lista de ibermon que estan en el equipo activo del jugador
        public void ObtenerEquipo(string partidaId,
            Action<List<IbermonJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/ibermon/equipo", ManejarListaEquipo, onError);

            // Funcion local que convierte el JSON en una lista de ibermon
            void ManejarListaEquipo(string respuestaJson)
            {
                List<IbermonJugador> equipo = ParsearLista(respuestaJson);
                onSuccess?.Invoke(equipo);
            }
        }

        // Obtiene la lista de ibermon guardados en el centro
        // Estos ibermon no pueden usarse en combate hasta que se muevan al equipo
        public void ObtenerCentro(string partidaId,
            Action<List<IbermonJugador>> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}/ibermon/centro", ManejarListaCentro, onError);

            // Funcion local que convierte el JSON en una lista de ibermon
            void ManejarListaCentro(string respuestaJson)
            {
                List<IbermonJugador> centro = ParsearLista(respuestaJson);
                onSuccess?.Invoke(centro);
            }
        }

        // Anade un ibermon nuevo al jugador cuando lo captura en un combate
        public void AnadirIbermon(string partidaId, IbermonJugadorCrearRequest datos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PostAuth($"/partidas/{partidaId}/ibermon/", JsonUtility.ToJson(datos),
                ManejarIbermonCreado, onError);

            // Funcion local que deserializa el ibermon creado y lo pasa al callback
            void ManejarIbermonCreado(string respuestaJson)
            {
                IbermonJugador ibermonNuevo = JsonUtility.FromJson<IbermonJugador>(respuestaJson);
                onSuccess?.Invoke(ibermonNuevo);
            }
        }

        // Mueve un ibermon entre el equipo y el centro
        public void MoverIbermon(string partidaId, string ibermonId, string ubicacion,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            // Preparamos la peticion con la nueva ubicacion
            IbermonJugadorMoverRequest peticion = new IbermonJugadorMoverRequest { ubicacion = ubicacion };

            Api.PatchAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/mover",
                JsonUtility.ToJson(peticion), ManejarIbermonMovido, onError);

            // Funcion local que deserializa el ibermon con la ubicacion actualizada
            void ManejarIbermonMovido(string respuestaJson)
            {
                IbermonJugador ibermonActualizado = JsonUtility.FromJson<IbermonJugador>(respuestaJson);
                onSuccess?.Invoke(ibermonActualizado);
            }
        }

        // Actualiza las estadisticas de un ibermon despues de un combate
        // Se envia el HP actual, nivel, experiencia y los movimientos aprendidos
        public void ActualizarIbermon(string partidaId, string ibermonId, IbermonJugadorActualizarRequest datos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PatchAuth($"/partidas/{partidaId}/ibermon/{ibermonId}",
                JsonUtility.ToJson(datos), ManejarIbermonActualizado, onError);

            // Funcion local que deserializa el ibermon con los nuevos datos
            void ManejarIbermonActualizado(string respuestaJson)
            {
                IbermonJugador ibermonActualizado = JsonUtility.FromJson<IbermonJugador>(respuestaJson);
                onSuccess?.Invoke(ibermonActualizado);
            }
        }

        // Libera un ibermon borrandolo de la partida
        public void EliminarIbermon(string partidaId, string ibermonId,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/ibermon/{ibermonId}", onSuccess, onError);
        }

        // Reemplaza la lista completa de movimientos de un ibermon
        // Se usa por ejemplo cuando el jugador decide cambiar los movimientos tras subir de nivel
        public void ActualizarMovimientos(string partidaId, string ibermonId,
            List<MovimientoAprendido> movimientos,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            // JsonUtility no puede serializar listas sueltas, asi que construimos el JSON a mano
            string jsonMovimientos = ConstruirJsonMovimientos(movimientos);

            Api.PutAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos",
                jsonMovimientos, ManejarMovimientosActualizados, onError);

            // Funcion local que deserializa el ibermon con los nuevos movimientos
            void ManejarMovimientosActualizados(string respuestaJson)
            {
                IbermonJugador ibermonActualizado = JsonUtility.FromJson<IbermonJugador>(respuestaJson);
                onSuccess?.Invoke(ibermonActualizado);
            }
        }

        // Hace que un ibermon aprenda un movimiento nuevo por su numero
        public void AprenderMovimiento(string partidaId, string ibermonId, int numeroMovimiento,
            Action<IbermonJugador> onSuccess, Action<string> onError)
        {
            Api.PostAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos/{numeroMovimiento}",
                "{}", ManejarMovimientoAprendido, onError);

            // Funcion local que deserializa el ibermon con el movimiento anadido
            void ManejarMovimientoAprendido(string respuestaJson)
            {
                IbermonJugador ibermonActualizado = JsonUtility.FromJson<IbermonJugador>(respuestaJson);
                onSuccess?.Invoke(ibermonActualizado);
            }
        }

        // Hace que un ibermon olvide un movimiento por su numero
        public void OlvidarMovimiento(string partidaId, string ibermonId, int numeroMovimiento,
            Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}/ibermon/{ibermonId}/movimientos/{numeroMovimiento}",
                onSuccess, onError);
        }

        // Metodo auxiliar que envuelve el JSON y lo convierte en una lista de IbermonJugador
        private List<IbermonJugador> ParsearLista(string respuestaJson)
        {
            // Envolvemos la lista en un objeto para que JsonUtility pueda leerla
            string jsonEnvuelto = "{\"items\":" + respuestaJson + "}";
            EnvoltorioLista envoltorio = JsonUtility.FromJson<EnvoltorioLista>(jsonEnvuelto);
            return envoltorio.items;
        }

        // Metodo auxiliar que construye a mano el JSON de una lista de movimientos
        // Se hace asi porque JsonUtility no sabe serializar listas en la raiz del JSON
        private string ConstruirJsonMovimientos(List<MovimientoAprendido> movimientos)
        {
            System.Text.StringBuilder constructorJson = new System.Text.StringBuilder("[");

            for (int indice = 0; indice < movimientos.Count; indice++)
            {
                // Si no es el primero anadimos una coma para separar los elementos
                if (indice > 0)
                {
                    constructorJson.Append(",");
                }

                MovimientoAprendido movimientoActual = movimientos[indice];
                constructorJson.Append($"{{\"numero\":{movimientoActual.numero},\"pp\":{movimientoActual.pp}}}");
            }

            constructorJson.Append("]");
            return constructorJson.ToString();
        }

        // Clase envoltorio privada para deserializar la lista de ibermon
        [Serializable]
        private class EnvoltorioLista
        {
            public List<IbermonJugador> items;
        }
    }
}

#pragma warning restore 0649
