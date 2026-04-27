using System;

namespace ApiRest.Models
{
    // Datos que se envian al servidor para registrar un nuevo usuario
    [Serializable]
    public class UsuarioRegistroRequest
    {
        public string username;
        public string email;
        public string password;
    }
}
