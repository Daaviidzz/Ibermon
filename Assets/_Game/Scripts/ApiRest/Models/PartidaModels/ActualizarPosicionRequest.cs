using System;

namespace ApiRest.Models
{
    // Peticion para actualizar la posicion del jugador, el tiempo jugado y la ultima conexion
    [Serializable]
    public class ActualizarPosicionRequest
    {
        public string mapa_actual;
        public Posicion posicion;
        public int tiempo_jugado;
        public string ultima_conexion; // ISO 8601, se genera en el momento de guardar
    }
}