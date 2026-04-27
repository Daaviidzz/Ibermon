using System;

namespace ApiRest.Models
{
    // Version completa de un movimiento con todos sus datos
    [Serializable]
    public class MovimientoCatalogoDetalle
    {
        public int numero;
        public string nombre;
        public string tipo;
        public int potencia;
        public int precision;
        public int pp;
        public string descripcion;

        // Efecto especial del movimiento. Puede ser nulo si no tiene
        public string efecto;

        // Categoria del movimiento. Puede ser Fisico, Especial o Estado
        public string categoria = "Fisico";

        // A quien afecta el movimiento. Foe es al rival y Self es a uno mismo
        public string objetivo = "Foe";

        public bool siempre_acierta = false;

        // Prioridad del movimiento. Cuanto mas alta antes se ejecuta en el turno
        public int prioridad = 0;
    }
}
