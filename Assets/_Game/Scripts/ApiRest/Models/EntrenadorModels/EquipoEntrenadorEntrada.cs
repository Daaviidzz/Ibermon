using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    [Serializable]
    public class EquipoEntrenadorEntrada
    {
        public int numero;
        public int nivel;
        public List<int> movs = new List<int>();
    }
}
