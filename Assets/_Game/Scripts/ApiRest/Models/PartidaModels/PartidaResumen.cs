using System;

namespace ApiRest.Models
{
    // Resumen de una partida para mostrarla en la lista de partidas guardadas
    // No trae todos los datos, solo los necesarios para el menu de seleccion
    [Serializable]
    public class PartidaResumen
    {
        public string id;
        public string personaje_elegido;
        public string mapa_actual;
        // Se usa como contador de veces que se ha entrado en la partida
        public int tiempo_jugado;
        public int combates_ganados;
        public int combates_perdidos;
    }
}
