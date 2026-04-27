using System;

namespace ApiRest.Models
{
    // Item concreto que tiene el jugador en su inventario
    [Serializable]
    public class ItemJugador
    {
        public string id;
        public string partida_id;
        public int item_catalogo_id;
        public int cantidad;
    }
}
