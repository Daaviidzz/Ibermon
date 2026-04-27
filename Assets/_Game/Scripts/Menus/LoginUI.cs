using ApiRest.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Este script controla toda la pantalla de inicio de sesion y registro del juego
// Se encarga de cambiar entre los paneles de inicio, login, registro y carga
// Y tambien de llamar a la API para iniciar sesion o crear una cuenta nueva
public class LoginUI : MonoBehaviour
{
    // Panel de inicio que es el primero que ve el jugador
    // Tiene los tres botones principales de Iniciar Sesion, Registrarse y Salir
    [Header("Panel Inicio")]
    public GameObject panelInicio;

    // Panel de login que aparece cuando el jugador pulsa Iniciar Sesion
    // Contiene los campos de usuario y contrasena mas un texto de error
    [Header("Panel Login")]
    public GameObject panelLogin;
    public TMP_InputField inputUsuarioLogin;
    public TMP_InputField inputPasswordLogin;
    public TextMeshProUGUI textoErrorLogin;

    // Panel de registro que aparece cuando el jugador pulsa Registrarse
    // Contiene los campos de correo, usuario y contrasena mas un texto de error
    [Header("Panel Registro")]
    public GameObject panelRegistro;
    public TMP_InputField inputCorreoRegistro;
    public TMP_InputField inputUsuarioRegistro;
    public TMP_InputField inputPasswordRegistro;
    public TextMeshProUGUI textoErrorRegistro;

    // Panel que se muestra mientras se espera una respuesta del servidor
    // Incluye un texto que le dice al jugador que esta pasando
    [Header("Panel Cargando")]
    public GameObject panelCargando;
    public TextMeshProUGUI textoCargando;

    // Nombre de la escena del menu principal a la que se pasa tras hacer login
    [Header("Configuracion general")]
    [Tooltip("Nombre exacto de la escena del menu principal tal y como aparece en Build Settings")]
    public string escenaMenuPrincipal = "MenuPrincipal";

    // Se ejecuta automaticamente al arrancar la escena
    private void Start()
    {
        // Al iniciar mostramos solo el panel de inicio y ocultamos el resto
        MostrarPanelInicio();

        // Comprobamos que el ApiManager exista para poder llamar a la API
        if (ApiManager.Instance == null)
        {
            Debug.LogError("[LoginUI] No se encontro el ApiManager. " +
                           "Asegurate de que el objeto ApiSetup esta en la escena anterior (Portada).");
        }
    }

    // Muestra el panel de inicio con los tres botones principales
    // Se conecta al boton Volver de los paneles de login y registro
    public void MostrarPanelInicio()
    {
        panelInicio.SetActive(true);
        panelLogin.SetActive(false);
        panelRegistro.SetActive(false);
        panelCargando.SetActive(false);

        // En el panel de inicio el cursor se oculta porque el jugador navega con botones
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Limpiamos los mensajes de error por si quedaron de un intento anterior
        LimpiarErrores();
    }

    // Muestra el panel de inicio de sesion
    // Se conecta al boton Iniciar Sesion del panel de inicio
    public void MostrarPanelLogin()
    {
        panelInicio.SetActive(false);
        panelLogin.SetActive(true);
        panelRegistro.SetActive(false);
        panelCargando.SetActive(false);

        // En los paneles con campos de texto el cursor debe ser visible para hacer clic
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LimpiarErrores();
    }

    // Muestra el panel de registro
    // Se conecta al boton Registrarse del panel de inicio
    public void MostrarPanelRegistro()
    {
        panelInicio.SetActive(false);
        panelLogin.SetActive(false);
        panelRegistro.SetActive(true);
        panelCargando.SetActive(false);

        // Igual que en el login necesitamos el cursor visible para los campos de texto
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LimpiarErrores();
    }

    // Muestra el panel de carga con un mensaje personalizado
    // Se llama mientras se espera la respuesta del servidor
    private void MostrarPanelCargando(string mensaje)
    {
        panelInicio.SetActive(false);
        panelLogin.SetActive(false);
        panelRegistro.SetActive(false);
        panelCargando.SetActive(true);

        // Actualizamos el texto para que el jugador sepa que esta pasando
        if (textoCargando != null)
        {
            textoCargando.text = mensaje;
        }

        // Mientras carga no hace falta el cursor, lo ocultamos para que quede mas limpio
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Cierra el juego
    // Se conecta al boton Salir del panel de inicio
    public void Salir()
    {
        Application.Quit();

        // Esta linea solo funciona dentro del editor de Unity
        // Es util para probar que el boton funciona sin cerrar Unity a mano
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Intenta iniciar sesion con los datos que ha escrito el jugador
    // Se conecta al boton Continuar del panel de Login
    public void OnClickLogin()
    {
        // Recogemos lo que ha escrito el jugador
        // Trim elimina los espacios del principio y del final del usuario
        string usuarioIntroducido = inputUsuarioLogin.text.Trim();
        string passwordIntroducida = inputPasswordLogin.text;

        // Comprobamos que no haya dejado ningun campo vacio
        if (string.IsNullOrEmpty(usuarioIntroducido) || string.IsNullOrEmpty(passwordIntroducida))
        {
            textoErrorLogin.text = "Por favor, introduce tu usuario y contrasena.";
            return;
        }

        // Mostramos la pantalla de carga mientras esperamos la respuesta del servidor
        MostrarPanelCargando("Iniciando sesion...");

        // Llamamos a la API para iniciar sesion pasandole dos funciones
        // Una se ejecuta si todo va bien y la otra si ocurre un error
        ApiSetup.Auth.Login(usuarioIntroducido, passwordIntroducida,
            ManejarLoginExitoso, ManejarLoginFallido);

        // Funcion local que se ejecuta si el login fue correcto
        // El parametro tokenRecibido no se usa aqui porque el ApiManager ya lo guarda
        void ManejarLoginExitoso(ApiRest.Models.TokenResponse tokenRecibido)
        {
            MostrarPanelCargando("Cargando datos del juego...");
            CargarDatosYEntrarAlJuego();
        }

        // Funcion local que se ejecuta si el login fallo
        // Muestra un mensaje distinto segun el codigo de error que devuelva el servidor
        void ManejarLoginFallido(string mensajeError)
        {
            MostrarPanelLogin();

            bool credencialesIncorrectas =
                mensajeError.Contains("[401]") || mensajeError.Contains("[422]");

            if (credencialesIncorrectas)
            {
                textoErrorLogin.text = "Usuario o contrasena incorrectos.";
            }
            else
            {
                textoErrorLogin.text = "Error de conexion. Intentalo de nuevo.";
            }
        }
    }

    // Intenta crear una cuenta nueva con los datos introducidos
    // Se conecta al boton Continuar del panel de Registro
    public void OnClickRegistrar()
    {
        // Recogemos los tres campos del formulario de registro
        string correoIntroducido = inputCorreoRegistro.text.Trim();
        string usuarioIntroducido = inputUsuarioRegistro.text.Trim();
        string passwordIntroducida = inputPasswordRegistro.text;

        // Comprobamos que no haya ningun campo vacio
        bool faltaAlgunCampo =
            string.IsNullOrEmpty(correoIntroducido) ||
            string.IsNullOrEmpty(usuarioIntroducido) ||
            string.IsNullOrEmpty(passwordIntroducida);

        if (faltaAlgunCampo)
        {
            textoErrorRegistro.text = "Por favor, rellena todos los campos.";
            return;
        }

        // Mostramos la pantalla de carga mientras se crea la cuenta
        MostrarPanelCargando("Registrando cuenta...");

        // Llamamos a la API para registrar la cuenta nueva
        ApiSetup.Auth.Registrar(usuarioIntroducido, correoIntroducido, passwordIntroducida,
            ManejarRegistroExitoso, ManejarRegistroFallido);

        // Funcion local que se ejecuta si el registro fue correcto
        // Despues del registro intentamos hacer login automaticamente
        // para no pedirle al jugador que introduzca los datos otra vez
        void ManejarRegistroExitoso(ApiRest.Models.UsuarioPublico usuarioCreado)
        {
            MostrarPanelCargando("Iniciando sesion...");

            ApiSetup.Auth.Login(usuarioIntroducido, passwordIntroducida,
                ManejarLoginAutomaticoExitoso, ManejarLoginAutomaticoFallido);
        }

        // Funcion local que se ejecuta cuando el login automatico tras el registro va bien
        void ManejarLoginAutomaticoExitoso(ApiRest.Models.TokenResponse tokenRecibido)
        {
            MostrarPanelCargando("Cargando datos del juego...");
            CargarDatosYEntrarAlJuego();
        }

        // Funcion local que se ejecuta si el login automatico fallo
        // El registro ya fue bien asi que mandamos al jugador al login manual
        void ManejarLoginAutomaticoFallido(string mensajeError)
        {
            MostrarPanelLogin();
            textoErrorLogin.text = "Cuenta creada. Ahora inicia sesion con tus datos.";
        }

        // Funcion local que se ejecuta si el registro fallo
        // Muestra un mensaje distinto segun el codigo de error del servidor
        void ManejarRegistroFallido(string mensajeError)
        {
            MostrarPanelRegistro();

            bool usuarioOCorreoYaExiste =
                mensajeError.Contains("[400]") ||
                mensajeError.Contains("[409]") ||
                mensajeError.Contains("[422]");

            if (usuarioOCorreoYaExiste)
            {
                textoErrorRegistro.text = "Ese usuario o correo ya esta en uso. Prueba con otro.";
            }
            else
            {
                textoErrorRegistro.text = "Error de conexion. Intentalo de nuevo.";
            }
        }
    }

    // Borra los mensajes de error de todos los paneles
    // Se llama cada vez que se cambia de panel para empezar limpios
    private void LimpiarErrores()
    {
        if (textoErrorLogin != null)
        {
            textoErrorLogin.text = "";
        }
        if (textoErrorRegistro != null)
        {
            textoErrorRegistro.text = "";
        }
    }

    // Una vez que el login o el registro son correctos cargamos los catalogos del juego
    // Cuando terminan de cargar pasamos a la escena del menu principal
    private void CargarDatosYEntrarAlJuego()
    {
        // Comprobamos que el sistema de cache de catalogos este disponible
        if (CatalogoCache.Instance == null)
        {
            Debug.LogError("[LoginUI] CatalogoCache no encontrado. Revisa que ApiSetup esta bien configurado.");
            MostrarPanelLogin();
            textoErrorLogin.text = "Error interno del juego. Contacta con el equipo.";
            return;
        }

        // Llamamos al cache para que descargue los catalogos desde el servidor
        CatalogoCache.Instance.CargarCatalogos(ManejarCatalogosCargados, ManejarErrorCargandoCatalogos);

        // Funcion local que se ejecuta cuando los catalogos terminan de descargarse
        // Pasamos a la escena del menu principal
        void ManejarCatalogosCargados()
        {
            SceneManager.LoadScene(escenaMenuPrincipal);
        }

        // Funcion local que se ejecuta si hubo un error descargando los catalogos
        // Volvemos al login con un mensaje explicativo
        void ManejarErrorCargandoCatalogos(string mensajeError)
        {
            MostrarPanelLogin();
            textoErrorLogin.text = "Error al cargar los datos del juego. Intentalo de nuevo.";
            Debug.LogError($"[LoginUI] Error cargando catalogos: {mensajeError}");
        }
    }
}
