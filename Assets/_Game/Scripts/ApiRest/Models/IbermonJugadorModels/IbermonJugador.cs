using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Ibermon concreto que posee el jugador
    // Este es el objeto completo tal como viene del servidor
    [Serializable]
    public class IbermonJugador
    {
        public string id;
        public string partida_id;
        public int ibermon_catalogo_id;
        public string nickname;
        public int nivel;
        public int experiencia;
        public int hp_actual;
        public string ubicacion;
        public List<MovimientoAprendido> movimientos_aprendidos = new List<MovimientoAprendido>();
    }
}
