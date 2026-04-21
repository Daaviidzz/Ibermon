using System;

namespace ApiRest.Models
{
    // Peticion para actualizar solo la posicion del jugador sin guardar el resto
    [Serializable]
    public class ActualizarPosicionRequest
    {
        public string mapa_actual;
        public Posicion posicion;
    }
}
