using System;
using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

#pragma warning disable 0649

namespace ApiRest.Services
{
    // Servicio que consulta el catalogo de entrenadores
    public class EntrenadorCatalogoService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        public void Listar(Action<List<EntrenadorCatalogoResumen>> onSuccess, Action<string> onError)
        {
            Api.Get("/catalogo/entrenadores", ManejarLista, onError);

            void ManejarLista(string json)
            {
                EntrenadorResumenWrapper envoltorio = EnvolverYDeserializar<EntrenadorResumenWrapper>(json);
                onSuccess?.Invoke(envoltorio.items);
            }
        }

        public void ObtenerPorNumero(int numero,
            Action<EntrenadorCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/entrenadores/{numero}", ManejarDetalle, onError);

            void ManejarDetalle(string json)
            {
                EntrenadorCatalogoDetalle detalle = JsonUtility.FromJson<EntrenadorCatalogoDetalle>(json);
                onSuccess?.Invoke(detalle);
            }
        }

        public void ObtenerPorNombre(string nombre,
            Action<EntrenadorCatalogoDetalle> onSuccess, Action<string> onError)
        {
            Api.Get($"/catalogo/entrenadores/por-nombre/{nombre}", ManejarDetalle, onError);

            void ManejarDetalle(string json)
            {
                EntrenadorCatalogoDetalle detalle = JsonUtility.FromJson<EntrenadorCatalogoDetalle>(json);
                onSuccess?.Invoke(detalle);
            }
        }

        private TEnvoltorio EnvolverYDeserializar<TEnvoltorio>(string jsonLista)
        {
            string jsonEnvuelto = "{\"items\":" + jsonLista + "}";
            return JsonUtility.FromJson<TEnvoltorio>(jsonEnvuelto);
        }

        [Serializable] private class EntrenadorResumenWrapper { public List<EntrenadorCatalogoResumen> items; }
    }
}

#pragma warning restore 0649
