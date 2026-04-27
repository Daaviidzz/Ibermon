using System;

namespace ApiRest.Models
{
    // Movimiento que un ibermon puede aprender y a que nivel lo aprende
    [Serializable]
    public class MovimientoPosible
    {
        public int numero;
        // Nivel minimo al que el ibermon aprende el movimiento
        public int nivel;
    }
}
