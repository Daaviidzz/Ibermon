using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ApiRest.Managers
{
    /// <summary>
    /// Singleton que gestiona todas las peticiones HTTP a la API de Ibermon.
    /// Almacena el JWT y lo añade automáticamente en las cabeceras.
    /// </summary>
    public class ApiManager : MonoBehaviour
    {
        public static ApiManager Instance { get; private set; }

        [Header("Configuración")]
        [SerializeField] private string baseUrl = "http://localhost:8000";

        private string _token;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_token);
        public string BaseUrl => baseUrl;

        // ------------------------------------------------------------------ //
        //  Lifecycle
        // ------------------------------------------------------------------ //

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ------------------------------------------------------------------ //
        //  Token
        // ------------------------------------------------------------------ //

        public void SetToken(string token) => _token = token;
        public void ClearToken() => _token = null;

        // ------------------------------------------------------------------ //
        //  HTTP helpers
        // ------------------------------------------------------------------ //

        /// <summary>GET sin autenticación.</summary>
        public Coroutine Get(string endpoint, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(UnityWebRequest.Get(Url(endpoint)), false, onSuccess, onError));

        /// <summary>GET con JWT.</summary>
        public Coroutine GetAuth(string endpoint, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(UnityWebRequest.Get(Url(endpoint)), true, onSuccess, onError));

        /// <summary>POST JSON con JWT.</summary>
        public Coroutine PostAuth(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("POST", endpoint, jsonBody), true, onSuccess, onError));

        /// <summary>POST JSON sin JWT (registro).</summary>
        public Coroutine Post(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("POST", endpoint, jsonBody), false, onSuccess, onError));

        /// <summary>POST form-data sin JWT (login OAuth2).</summary>
        public Coroutine PostForm(string endpoint, WWWForm form, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(UnityWebRequest.Post(Url(endpoint), form), false, onSuccess, onError));

        /// <summary>PUT JSON con JWT.</summary>
        public Coroutine PutAuth(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("PUT", endpoint, jsonBody), true, onSuccess, onError));

        /// <summary>PATCH JSON con JWT.</summary>
        public Coroutine PatchAuth(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("PATCH", endpoint, jsonBody), true, onSuccess, onError));

        /// <summary>DELETE con JWT.</summary>
        public Coroutine DeleteAuth(string endpoint, Action onSuccess, Action<string> onError)
            => StartCoroutine(SendDelete(Url(endpoint), onSuccess, onError));

        // ------------------------------------------------------------------ //
        //  Internals
        // ------------------------------------------------------------------ //

        private string Url(string endpoint) => baseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');

        private UnityWebRequest BuildJsonRequest(string method, string endpoint, string json)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            var req = new UnityWebRequest(Url(endpoint), method);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            return req;
        }

        private void AddAuthHeader(UnityWebRequest req)
        {
            if (!string.IsNullOrEmpty(_token))
                req.SetRequestHeader("Authorization", "Bearer " + _token);
        }

        private IEnumerator SendRequest(UnityWebRequest req, bool withAuth, Action<string> onSuccess, Action<string> onError)
        {
            if (req.downloadHandler == null)
                req.downloadHandler = new DownloadHandlerBuffer();

            if (withAuth) AddAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
            else
            {
                string err = $"[{req.responseCode}] {req.downloadHandler?.text ?? req.error}";
                Debug.LogWarning($"ApiManager error en {req.url}: {err}");
                onError?.Invoke(err);
            }
        }

        private IEnumerator SendDelete(string url, Action onSuccess, Action<string> onError)
        {
            var req = UnityWebRequest.Delete(url);
            req.downloadHandler = new DownloadHandlerBuffer();
            AddAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success || req.responseCode == 204)
                onSuccess?.Invoke();
            else
                onError?.Invoke($"[{req.responseCode}] {req.downloadHandler?.text ?? req.error}");
        }
    }
}
