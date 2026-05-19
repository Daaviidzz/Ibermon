using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Datos para actualizar un ibermon del jugador despues de un combate
    // JsonUtility no serializa bien Nullable<T>; este request se usa para
    // sincronizar el estado completo del ibermon tras cada combate.
    [Serializable]
    public class IbermonJugadorActualizarRequest
    {
        public int nivel;
        public int experiencia;
        public int hp_actual;
        public List<MovimientoAprendido> movimientos_aprendidos;
        public string nickname;
    }
}
