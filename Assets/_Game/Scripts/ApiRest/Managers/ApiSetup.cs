using ApiRest.Services;
using UnityEngine;

namespace ApiRest.Managers
{
    // Este script se ejecuta antes que los demas gracias al atributo DefaultExecutionOrder
    // Se encarga de crear el ApiManager y todos los servicios de la API al iniciar el juego
    [DefaultExecutionOrder(-100)]
    public class ApiSetup : MonoBehaviour
    {
        // Direccion del servidor FastAPI que se configura desde el inspector
        [Header("URL del servidor FastAPI")]
        [SerializeField] private string urlServidor = "http://localhost:8000";

        // Servicios que podran usar el resto de scripts del juego
        public static AuthService Auth { get; private set; }
        public static PartidaService Partida { get; private set; }
        public static IbermonJugadorService IbermonJugador { get; private set; }
        public static ItemJugadorService ItemJugador { get; private set; }
        public static CatalogoService Catalogo { get; private set; }

        private void Awake()
        {
            // Si ya hay otro ApiSetup en la escena destruimos este para evitar duplicados
            ApiSetup[] copiasExistentes = FindObjectsByType<ApiSetup>(FindObjectsInactive.Exclude);
            if (copiasExistentes.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            // Hacemos que este objeto no se destruya al cambiar de escena
            DontDestroyOnLoad(gameObject);

            // Creamos un unico GameObject que agrupa todos los servicios
            // Asi la jerarquia queda mas limpia y ordenada
            GameObject contenedorServicios = new GameObject("_ApiInfrastructure");
            DontDestroyOnLoad(contenedorServicios);

            // Anadimos el ApiManager al contenedor
            ApiManager apiManager = contenedorServicios.AddComponent<ApiManager>();

            // El ApiManager tiene una variable privada baseUrl
            // Usamos reflexion para asignarle la URL que tenemos en el inspector
            System.Reflection.FieldInfo campoUrlPrivado = typeof(ApiManager).GetField("baseUrl",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (campoUrlPrivado != null)
            {
                campoUrlPrivado.SetValue(apiManager, urlServidor);
            }

            // Anadimos todos los servicios de la API al contenedor
            Auth = contenedorServicios.AddComponent<AuthService>();
            Partida = contenedorServicios.AddComponent<PartidaService>();
            IbermonJugador = contenedorServicios.AddComponent<IbermonJugadorService>();
            ItemJugador = contenedorServicios.AddComponent<ItemJugadorService>();
            Catalogo = contenedorServicios.AddComponent<CatalogoService>();

            // Anadimos tambien el gestor de sesion y el cache de catalogos
            contenedorServicios.AddComponent<SessionManager>();
            contenedorServicios.AddComponent<CatalogoCache>();

            Debug.Log("ApiSetup listo");
        }
    }
}
