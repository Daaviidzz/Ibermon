using System;

//para la nueva partida (la petición post basicamente)
namespace ApiRest.Models
{
        [Serializable]
        public class PartidaNuevaRequest
        {
            public string nombre;
            public string personaje_elegido;
        }
}