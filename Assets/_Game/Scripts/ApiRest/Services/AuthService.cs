using System;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    /// <summary>
    /// Endpoints: POST /auth/registro, POST /auth/login, GET /auth/yo
    /// </summary>
    public class AuthService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        // ------------------------------------------------------------------ //
        //  POST /auth/registro
        // ------------------------------------------------------------------ //

        public void Registrar(string username, string email, string password,
            Action<UsuarioPublico> onSuccess, Action<string> onError)
        {
            var body = new UsuarioRegistroRequest
            {
                username = username,
                email = email,
                password = password
            };
            string json = JsonUtility.ToJson(body);
            Api.Post("/auth/registro", json,
                raw => onSuccess?.Invoke(JsonUtility.FromJson<UsuarioPublico>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  POST /auth/login  (OAuth2 form-data)
        // ------------------------------------------------------------------ //

        public void Login(string username, string password,
            Action<TokenResponse> onSuccess, Action<string> onError)
        {
            // FastAPI espera form-data (OAuth2PasswordRequestForm), no JSON
            var form = new UnityEngine.WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);

            Api.PostForm("/auth/login", form,
                raw =>
                {
                    var token = JsonUtility.FromJson<TokenResponse>(raw);
                    Api.SetToken(token.access_token);
                    onSuccess?.Invoke(token);
                },
                onError);
        }

        // ------------------------------------------------------------------ //
        //  GET /auth/yo
        // ------------------------------------------------------------------ //

        public void ObtenerUsuarioActual(Action<UsuarioPublico> onSuccess, Action<string> onError)
        {
            Api.GetAuth("/auth/yo",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<UsuarioPublico>(raw)),
                onError);
        }

        // ------------------------------------------------------------------ //
        //  Logout (local — borra el token)
        // ------------------------------------------------------------------ //

        public void Logout() => Api.ClearToken();
    }
}
