using ApiRest.Services;
using UnityEngine;

namespace ApiRest.Managers
{
    /// <summary>
    /// Añade este componente a un GameObject vacío en la primera escena.
    /// Crea el ApiManager singleton y todos los servicios en un mismo objeto.
    ///
    /// Uso en Inspector:
    ///   - Base Url: http://localhost:8000  (local)
    ///               http://TU_IP:8000      (red local)
    ///               https://api.ibermon.com (producción)
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
            // Crea el ApiManager y le asigna la URL
            var go = new GameObject("ApiManager");
            DontDestroyOnLoad(go);

            var mgr = go.AddComponent<ApiManager>();
            // Usamos reflexión para asignar el campo privado serializado
            var field = typeof(ApiManager).GetField("baseUrl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(mgr, baseUrl);

            Auth           = go.AddComponent<AuthService>();
            Partida        = go.AddComponent<PartidaService>();
            IbermonJugador = go.AddComponent<IbermonJugadorService>();
            ItemJugador    = go.AddComponent<ItemJugadorService>();
            Catalogo       = go.AddComponent<CatalogoService>();
        }
    }
}
