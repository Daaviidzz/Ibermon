using System;

namespace ApiRest.Models
{
    // --- REQUESTS ---

    [Serializable]
    public class ItemJugadorAnadirRequest
    {
        public int item_catalogo_id;
        public int cantidad = 1;
    }

    [Serializable]
    public class ItemJugadorActualizarRequest
    {
        public int cantidad;
    }

    // --- RESPONSE ---

    [Serializable]
    public class ItemJugador
    {
        public string id;
        public string partida_id;
        public int item_catalogo_id;
        public int cantidad;
    }
}
