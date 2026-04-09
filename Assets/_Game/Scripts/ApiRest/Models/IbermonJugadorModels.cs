using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // --- REQUESTS ---

    [Serializable]
    public class IbermonJugadorCrearRequest
    {
        public int ibermon_catalogo_id;
        public string nickname;       // opcional, puede ser null
        public int nivel = 1;
        public int hp_actual;
        public string ubicacion = "equipo";
    }

    [Serializable]
    public class IbermonJugadorMoverRequest
    {
        public string ubicacion;      // "equipo" o "centro"
    }

    [Serializable]
    public class IbermonJugadorActualizarRequest
    {
        public int? nivel;
        public int? experiencia;
        public int? hp_actual;
        public List<int> movimientos_aprendidos;
        public string nickname;
    }

    // --- RESPONSE ---

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
        public List<int> movimientos_aprendidos = new List<int>();
    }
}
