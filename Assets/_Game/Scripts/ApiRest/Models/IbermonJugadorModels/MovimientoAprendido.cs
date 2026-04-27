using System;

namespace ApiRest.Models
{
    // Movimiento que un ibermon del jugador ya tiene aprendido
    [Serializable]
    public class MovimientoAprendido
    {
        public int numero;
        // PP actuales que le quedan al movimiento
        public int pp;
    }
}
