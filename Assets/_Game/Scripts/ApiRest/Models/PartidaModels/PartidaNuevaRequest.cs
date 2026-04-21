using System;

namespace ApiRest.Models
{
    // Datos para crear una nueva partida
    [Serializable]
    public class PartidaNuevaRequest
    {
        public string personaje_elegido;
        public int starter_elegido;
    }
}
