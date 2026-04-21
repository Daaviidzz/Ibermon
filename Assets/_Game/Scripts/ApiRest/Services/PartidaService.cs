using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    // Servicio que gestiona las partidas del jugador en el servidor
    // Endpoints:
    //   POST   /partidas/
    //   GET    /partidas/
    //   GET    /partidas/{id}
    //   PUT    /partidas/{id}/guardar
    //   PATCH  /partidas/{id}/posicion
    //   DELETE /partidas/{id}
    public class PartidaService : MonoBehaviour
    {
        // Acceso rapido al ApiManager
        private ApiManager Api => ApiManager.Instance;

        // Crea una partida nueva en el servidor con el personaje y starter elegidos
        public void CrearPartida(string personajeElegido, int starterElegido,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            // Preparamos la peticion con los datos iniciales de la partida
            PartidaNuevaRequest peticion = new PartidaNuevaRequest
            {
                personaje_elegido = personajeElegido,
                starter_elegido = starterElegido
            };

            Api.PostAuth("/partidas/", JsonUtility.ToJson(peticion),
                ManejarPartidaCreada, onError);

            // Funcion local que deserializa la partida recien creada
            void ManejarPartidaCreada(string respuestaJson)
            {
                PartidaCompleta partidaNueva = JsonUtility.FromJson<PartidaCompleta>(respuestaJson);
                onSuccess?.Invoke(partidaNueva);
            }
        }

        // Obtiene la lista de partidas guardadas del usuario actual
        // Devuelve solo un resumen de cada una, no los datos completos
        public void ListarPartidas(Action<List<PartidaResumen>> onSuccess, Action<string> onError)
        {
            Api.GetAuth("/partidas/", ManejarListaPartidas, onError);

            // Funcion local que envuelve el JSON y extrae la lista de partidas
            void ManejarListaPartidas(string respuestaJson)
            {
                // JsonUtility no puede deserializar listas sueltas
                // Por eso las envolvemos dentro de un objeto con campo items
                string jsonEnvuelto = "{\"items\":" + respuestaJson + "}";
                PartidaResumenListWrapper envoltorio =
                    JsonUtility.FromJson<PartidaResumenListWrapper>(jsonEnvuelto);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        // Obtiene los datos completos de una partida concreta por su id
        public void ObtenerPartida(string partidaId,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            Api.GetAuth($"/partidas/{partidaId}", ManejarPartidaCompleta, onError);

            // Funcion local que deserializa la partida completa
            void ManejarPartidaCompleta(string respuestaJson)
            {
                PartidaCompleta partida = JsonUtility.FromJson<PartidaCompleta>(respuestaJson);
                onSuccess?.Invoke(partida);
            }
        }

        // Guarda el estado completo de una partida en el servidor
        // Se usa al guardar manualmente o al cerrar el juego
        public void GuardarPartida(string partidaId, GuardarPartidaRequest datos,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            Api.PutAuth($"/partidas/{partidaId}/guardar", JsonUtility.ToJson(datos),
                ManejarPartidaGuardada, onError);

            // Funcion local que deserializa la partida ya actualizada
            void ManejarPartidaGuardada(string respuestaJson)
            {
                PartidaCompleta partidaGuardada = JsonUtility.FromJson<PartidaCompleta>(respuestaJson);
                onSuccess?.Invoke(partidaGuardada);
            }
        }

        // Actualiza solo la posicion del jugador en el mapa
        // Es una llamada mas ligera que guardar toda la partida
        public void ActualizarPosicion(string partidaId, string mapaActual, float x, float y,
            Action<PartidaCompleta> onSuccess, Action<string> onError)
        {
            // Preparamos la peticion con el mapa y la nueva posicion
            ActualizarPosicionRequest peticion = new ActualizarPosicionRequest
            {
                mapa_actual = mapaActual,
                posicion = new Posicion { x = x, y = y }
            };

            Api.PatchAuth($"/partidas/{partidaId}/posicion", JsonUtility.ToJson(peticion),
                ManejarPosicionActualizada, onError);

            // Funcion local que deserializa la partida con la nueva posicion
            void ManejarPosicionActualizada(string respuestaJson)
            {
                PartidaCompleta partidaActualizada = JsonUtility.FromJson<PartidaCompleta>(respuestaJson);
                onSuccess?.Invoke(partidaActualizada);
            }
        }

        // Borra una partida del servidor de forma permanente
        public void EliminarPartida(string partidaId, Action onSuccess, Action<string> onError)
        {
            Api.DeleteAuth($"/partidas/{partidaId}", onSuccess, onError);
        }

        // Clase envoltorio privada para deserializar la lista de resumenes de partidas
        [Serializable]
        private class PartidaResumenListWrapper
        {
            public List<PartidaResumen> items;
        }
    }
}
