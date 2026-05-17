using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ApiRest.Managers
{
    // Todas las peticiones HTTP pasan por aquí.
    // Guarda el JWT después del login y lo mete solo en cada cabecera.
    public class ApiManager : MonoBehaviour
    {
        public static ApiManager Instance { get; private set; }

        [Header("Configuración")]
        [SerializeField] private string baseUrl = "http://localhost:8000";

        private string _token;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_token);
        public string BaseUrl => baseUrl;

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

        public void SetToken(string token) => _token = token;
        public void ClearToken() => _token = null;
        public void SetBaseUrl(string nuevaBaseUrl) => baseUrl = nuevaBaseUrl;

        // GET público (catálogos, etc.)
        public Coroutine Get(string endpoint, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(UnityWebRequest.Get(Url(endpoint)), false, onSuccess, onError));

        // GET con JWT
        public Coroutine GetAuth(string endpoint, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(UnityWebRequest.Get(Url(endpoint)), true, onSuccess, onError));

        // POST JSON con JWT
        public Coroutine PostAuth(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("POST", endpoint, jsonBody), true, onSuccess, onError));

        // POST JSON sin JWT — solo para registro
        public Coroutine Post(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("POST", endpoint, jsonBody), false, onSuccess, onError));

        // POST form-data sin JWT — el login de FastAPI usa OAuth2 form-data, no JSON
        public Coroutine PostForm(string endpoint, WWWForm form, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(UnityWebRequest.Post(Url(endpoint), form), false, onSuccess, onError));

        // PUT JSON con JWT
        public Coroutine PutAuth(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("PUT", endpoint, jsonBody), true, onSuccess, onError));

        // PATCH JSON con JWT
        public Coroutine PatchAuth(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
            => StartCoroutine(SendRequest(BuildJsonRequest("PATCH", endpoint, jsonBody), true, onSuccess, onError));

        // DELETE con JWT
        public Coroutine DeleteAuth(string endpoint, Action onSuccess, Action<string> onError)
            => StartCoroutine(SendDelete(Url(endpoint), onSuccess, onError));

        private string Url(string endpoint) => baseUrl.TrimEnd('/') + "/" + endpoint.TrimStart('/');

        private UnityWebRequest BuildJsonRequest(string method, string endpoint, string json)
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            var req = new UnityWebRequest(Url(endpoint), method);
            req.uploadHandler   = new UploadHandlerRaw(body);
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

            // 204 No Content es éxito en un DELETE
            if (req.result == UnityWebRequest.Result.Success || req.responseCode == 204)
                onSuccess?.Invoke();
            else
                onError?.Invoke($"[{req.responseCode}] {req.downloadHandler?.text ?? req.error}");
        }
    }
}
