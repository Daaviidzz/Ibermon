using System;

namespace ApiRest.Models
{
    // Datos para anadir un item nuevo al inventario del jugador
    [Serializable]
    public class ItemJugadorAnadirRequest
    {
        public int item_catalogo_id;
        public int cantidad = 1;
    }
}
