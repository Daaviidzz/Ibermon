using System;

namespace ApiRest.Models
{
    // Datos para actualizar la cantidad de un item del inventario
    [Serializable]
    public class ItemJugadorActualizarRequest
    {
        public int cantidad;
    }
}
