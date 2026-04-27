using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Version completa de un ibermon del catalogo con todos sus datos
    // Se pide cuando necesitamos toda la informacion de un ibermon concreto
    [Serializable]
    public class IbermonCatalogoDetalle
    {
        public int numero;
        public string nombre;
        public string tipo1;
        public string tipo2;
        public string descripcion;

        // Estadisticas base que se usan en los calculos del combate
        public int hp_base;
        public int ataque_base;
        public int defensa_base;
        public int ataque_especial_base;
        public int defensa_especial_base;
        public int velocidad_base;

        // Lista de movimientos que puede aprender
        public List<MovimientoPosible> movimientos_posibles = new List<MovimientoPosible>();

        // Datos de evolucion. Pueden ser nulos si el ibermon no evoluciona
        public int? evoluciona_a;
        public int? nivel_evolucion;

        public string sprite;

        // Probabilidad de captura. Cuanto mas alto mas facil es atraparlo
        public int catch_rate = 255;

        // Experiencia que da al ser derrotado
        public int exp_yield = 100;

        // Ritmo de crecimiento. Puede ser Medio o Rapido
        public string growth_rate = "Medio";
    }
}
