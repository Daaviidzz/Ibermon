using System;

namespace ApiRest.Models
{
    // Datos para crear un nuevo ibermon del jugador al atrapar uno
    [Serializable]
    public class IbermonJugadorCrearRequest
    {
        public int ibermon_catalogo_id;
        // Nombre personalizado que le pone el jugador. Puede ser nulo
        public string nickname;
        public int nivel = 1;
        public int hp_actual;
        public int hp_maximo;
        // Donde se guarda el ibermon. Por defecto en el equipo
        public string ubicacion = "equipo";
    }
}
