using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // Informacion publica de un usuario que devuelve el servidor
    // No contiene la contrasena ni datos sensibles
    [Serializable]
    public class UsuarioPublico
    {
        public string id;
        public string username;
        public string email;
        public string fecha_registro;
        public List<string> partidas = new List<string>();
    }
}
