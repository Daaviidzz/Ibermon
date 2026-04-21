using System;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

namespace ApiRest.Services
{
    // Servicio que se encarga de la autenticacion del usuario
    // Habla con los endpoints POST /auth/registro, POST /auth/login y GET /auth/yo
    public class AuthService : MonoBehaviour
    {
        // Propiedad corta para acceder al ApiManager
        private ApiManager Api => ApiManager.Instance;

        // Registra un usuario nuevo en el servidor
        // Recibe el username, email y password del formulario de registro
        // Llama a onSuccess con los datos publicos del usuario creado
        // Llama a onError con un mensaje si algo falla
        public void Registrar(string username, string email, string password,
            Action<UsuarioPublico> onSuccess, Action<string> onError)
        {
            // Preparamos el objeto con los datos del nuevo usuario
            UsuarioRegistroRequest datosRegistro = new UsuarioRegistroRequest
            {
                username = username,
                email = email,
                password = password
            };

            // Enviamos la peticion POST con el cuerpo en formato JSON
            Api.Post("/auth/registro", JsonUtility.ToJson(datosRegistro),
                ManejarRespuestaRegistro, onError);

            // Funcion local que convierte la respuesta JSON en un UsuarioPublico
            // y la pasa al callback de exito
            void ManejarRespuestaRegistro(string respuestaJson)
            {
                UsuarioPublico usuarioCreado = JsonUtility.FromJson<UsuarioPublico>(respuestaJson);
                onSuccess?.Invoke(usuarioCreado);
            }
        }

        // Inicia sesion en el servidor y guarda el token JWT en el ApiManager
        // FastAPI usa OAuth2PasswordRequestForm que requiere form-data, no JSON
        // Por eso se usa WWWForm en vez de enviar un objeto serializado
        public void Login(string username, string password,
            Action<TokenResponse> onSuccess, Action<string> onError)
        {
            // Preparamos el formulario con los campos username y password
            WWWForm formularioLogin = new WWWForm();
            formularioLogin.AddField("username", username);
            formularioLogin.AddField("password", password);

            // Enviamos el formulario al endpoint de login
            Api.PostForm("/auth/login", formularioLogin, ManejarRespuestaLogin, onError);

            // Funcion local que guarda el token y avisa al callback
            void ManejarRespuestaLogin(string respuestaJson)
            {
                TokenResponse token = JsonUtility.FromJson<TokenResponse>(respuestaJson);
                // Guardamos el token en el ApiManager para que se envie en las proximas peticiones
                Api.SetToken(token.access_token);
                onSuccess?.Invoke(token);
            }
        }

        // Pide al servidor los datos del usuario que esta conectado ahora mismo
        // Necesita que ya haya un token JWT guardado
        public void ObtenerUsuarioActual(Action<UsuarioPublico> onSuccess, Action<string> onError)
        {
            Api.GetAuth("/auth/yo", ManejarRespuestaUsuario, onError);

            // Funcion local que convierte el JSON recibido en un UsuarioPublico
            void ManejarRespuestaUsuario(string respuestaJson)
            {
                UsuarioPublico usuario = JsonUtility.FromJson<UsuarioPublico>(respuestaJson);
                onSuccess?.Invoke(usuario);
            }
        }

        // Cierra la sesion de forma local borrando el token guardado
        // No hace ninguna llamada a la API porque el JWT es autocontenido
        public void Logout()
        {
            Api.ClearToken();
        }
    }
}
