using System;
using System.Collections.Generic;

namespace ApiRest.Models
{
    // --- REQUESTS ---

    [Serializable]
    public class UsuarioRegistroRequest
    {
        public string username;
        public string email;
        public string password;
    }

    // Login se envía como form-data (OAuth2PasswordRequestForm), no JSON.
    // Ver AuthService.Login para el manejo especial.

    // --- RESPONSES ---

    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string token_type;
    }

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
