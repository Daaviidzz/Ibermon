using System;

namespace ApiRest.Models
{
    // Peticion para mover un ibermon entre el equipo y el centro
    [Serializable]
    public class IbermonJugadorMoverRequest
    {
        // Destino del ibermon. Puede ser equipo o centro
        public string ubicacion;
    }
}
