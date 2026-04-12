using System;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    // Endpoints: POST /auth/registro, POST /auth/login, GET /auth/yo
    public class AuthService : MonoBehaviour
    {
        private ApiManager Api => ApiManager.Instance;

        public void Registrar(string username, string email, string password,
            Action<UsuarioPublico> onSuccess, Action<string> onError)
        {
            var body = new UsuarioRegistroRequest
            {
                username = username,
                email    = email,
                password = password
            };
            Api.Post("/auth/registro", JsonUtility.ToJson(body),
                raw => onSuccess?.Invoke(JsonUtility.FromJson<UsuarioPublico>(raw)),
                onError);
        }

        public void Login(string username, string password,
            Action<TokenResponse> onSuccess, Action<string> onError)
        {
            // FastAPI usa OAuth2PasswordRequestForm, que es form-data, no JSON
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

        public void ObtenerUsuarioActual(Action<UsuarioPublico> onSuccess, Action<string> onError)
        {
            Api.GetAuth("/auth/yo",
                raw => onSuccess?.Invoke(JsonUtility.FromJson<UsuarioPublico>(raw)),
                onError);
        }

        // Logout es solo local — borra el token de memoria, no llama a la API
        public void Logout() => Api.ClearToken();
    }
}
