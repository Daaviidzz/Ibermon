using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Datos para actualizar un ibermon del jugador despues de un combate
    // Los campos con ? son opcionales y se envian nulos si no cambian
    [Serializable]
    public class IbermonJugadorActualizarRequest
    {
        public int? nivel;
        public int? experiencia;
        public int? hp_actual;
        public List<MovimientoAprendido> movimientos_aprendidos;
        public string nickname;
    }
}
