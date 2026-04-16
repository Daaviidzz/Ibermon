using ApiRest.Services;
using UnityEngine;

namespace ApiRest.Managers
{

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
            // Evitar que se duplique si la escena se recarga
            if (FindObjectsByType<ApiSetup>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            // Un solo GameObject agrupa todos los servicios para mantener la jerarquía limpia
            var go = new GameObject("_ApiInfrastructure");
            DontDestroyOnLoad(go);

            // ApiManager necesita la baseUrl, que está en el Inspector de este componente
            var mgr = go.AddComponent<ApiManager>();
            var field = typeof(ApiManager).GetField("baseUrl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(mgr, baseUrl);

            Auth = go.AddComponent<AuthService>();
            Partida = go.AddComponent<PartidaService>();
            IbermonJugador = go.AddComponent<IbermonJugadorService>();
            ItemJugador = go.AddComponent<ItemJugadorService>();
            Catalogo = go.AddComponent<CatalogoService>();

            go.AddComponent<SessionManager>();
            go.AddComponent<CatalogoCache>();

            Debug.Log("ApiSetup listo");
        }
    }
}
