using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    [Serializable]
    public class EntrenadorCatalogoDetalle
    {
        public int numero;
        public string nombre;
        public List<EquipoEntrenadorEntrada> equipo = new List<EquipoEntrenadorEntrada>();
        public int recompensa;
        public DialogosEntrenador dialogos;
        public string sprite;
    }
}
