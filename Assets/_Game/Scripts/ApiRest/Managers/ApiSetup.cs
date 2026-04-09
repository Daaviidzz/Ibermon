using ApiRest.Services;
using UnityEngine;

namespace ApiRest.Managers
{
    /// <summary>
    /// Añade este componente a un GameObject vacío en la primera escena (Portada o Login).
    /// Crea el ApiManager singleton, todos los servicios, el SessionManager y el CatalogoCache
    /// con DontDestroyOnLoad, por lo que solo hace falta añadirlo UNA VEZ.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ApiSetup : MonoBehaviour
    {
        [Header("URL del servidor FastAPI")]
        [SerializeField] private string baseUrl = "http://localhost:8000";

        public static AuthService Auth { get; private set; }
        public static PartidaService Partida { get; private set; }
        public static IbermonJugadorService IbermonJugador { get; private set; }
        public static ItemJugadorService ItemJugador { get; private set; }
        public static CatalogoService Catalogo { get; private set; }

        private void Awake()
        {
            // Evitar duplicados si la escena se recarga (Unity moderno)
            if (FindObjectsByType<ApiSetup>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            // Crear el GameObject persistente que aloja todos los servicios
            var go = new GameObject("_ApiInfrastructure");
            DontDestroyOnLoad(go);

            // ApiManager (HTTP + token)
            var mgr = go.AddComponent<ApiManager>();
            var field = typeof(ApiManager).GetField("baseUrl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(mgr, baseUrl);

            // Servicios REST
            Auth = go.AddComponent<AuthService>();
            Partida = go.AddComponent<PartidaService>();
            IbermonJugador = go.AddComponent<IbermonJugadorService>();
            ItemJugador = go.AddComponent<ItemJugadorService>();
            Catalogo = go.AddComponent<CatalogoService>();

            // Infraestructura de sesión
            go.AddComponent<SessionManager>();
            go.AddComponent<CatalogoCache>();

            Debug.Log("✅ ApiSetup inicializado correctamente");
        }
    }
}