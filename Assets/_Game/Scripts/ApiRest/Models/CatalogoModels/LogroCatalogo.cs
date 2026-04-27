using System;

namespace ApiRest.Models
{
    // Logro que el jugador puede desbloquear cumpliendo una condicion
    [Serializable]
    public class LogroCatalogo
    {
        public string codigo;
        public string nombre;
        public string descripcion;
        public string condicion;
        public string icono;
    }
}
