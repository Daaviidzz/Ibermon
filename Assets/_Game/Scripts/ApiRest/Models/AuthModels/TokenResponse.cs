using System;

namespace ApiRest.Models
{
    // Respuesta que devuelve el servidor al hacer login correctamente
    // Contiene el token JWT que despues se usa en las cabeceras de autenticacion
    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string token_type;
    }
}
