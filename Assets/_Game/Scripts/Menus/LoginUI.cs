using ApiRest.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Este script controla toda la pantalla de inicio de sesión y registro del juego.
/// 
/// ¿Cómo funciona la navegación?
///   - Al arrancar la escena, se muestra el PanelInicio (con los 3 botones principales)
///   - Si el jugador pulsa "Iniciar Sesión" → se oculta PanelInicio y aparece PaneLogin
///   - Si el jugador pulsa "Registrarse"    → se oculta PanelInicio y aparece PaneRegistro
///   - En cualquier panel, el botón "Volver" regresa al PanelInicio
///   - Si los datos son correctos y se pulsa "Continuar" → carga la escena del menú principal
///   - Si hay algún error (contraseña mal, usuario ya existe...) → aparece el mensaje en TextoError
/// 
/// ¿Cómo conectarlo en Unity?
///   1. Adjunta este script a cualquier GameObject vacío de la escena (por ejemplo, uno llamado "UIManager")
///   2. Arrastra cada objeto de la jerarquía al hueco correspondiente en el Inspector
///   3. Conecta los botones: en el evento OnClick() de cada botón, arrastra el GameObject
///      que tiene este script y selecciona la función correspondiente
/// </summary>
public class LoginUI : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // PANEL INICIO
    // Es el primer panel que ve el jugador, con tres botones: Iniciar Sesión,
    // Registrarse y Salir.
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Panel Inicio - El menú principal con los 3 botones")]
    public GameObject panelInicio;
    // Nota: los botones IniciarSesion, Registrarse y Salir se conectan
    // directamente desde el Inspector de Unity (OnClick), no hace falta
    // declararlos aquí porque solo cambian de panel.


    // ─────────────────────────────────────────────────────────────────────────
    // PANEL LOGIN
    // Aparece cuando el jugador pulsa "Iniciar Sesión".
    // Contiene: campo usuario, campo contraseña, botón Continuar,
    // botón Volver y un texto para mostrar errores.
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Panel Login - Pantalla de inicio de sesión")]
    public GameObject panelLogin;
    public TMP_InputField inputUsuarioLogin;    // Campo donde se escribe el nombre de usuario
    public TMP_InputField inputPasswordLogin;   // Campo donde se escribe la contraseña
    public TextMeshProUGUI textoErrorLogin;      // Texto rojo que aparece si algo sale mal


    // ─────────────────────────────────────────────────────────────────────────
    // PANEL REGISTRO
    // Aparece cuando el jugador pulsa "Registrarse".
    // Contiene: campo correo, campo usuario, campo contraseña, botón Continuar,
    // botón Volver y un texto para mostrar errores.
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Panel Registro - Pantalla de creación de cuenta")]
    public GameObject panelRegistro;
    public TMP_InputField inputCorreoRegistro;   // Campo donde se escribe el correo electrónico
    public TMP_InputField inputUsuarioRegistro;  // Campo donde se escribe el nombre de usuario
    public TMP_InputField inputPasswordRegistro; // Campo donde se escribe la contraseña
    public TextMeshProUGUI textoErrorRegistro;    // Texto rojo que aparece si algo sale mal


    // ─────────────────────────────────────────────────────────────────────────
    // CONFIGURACIÓN GENERAL
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Configuración general")]
    [Tooltip("Escribe aquí el nombre exacto de la escena del menú principal (tal como aparece en Build Settings)")]
    public string escenaMenuPrincipal = "MenuPrincipal";


    // ─────────────────────────────────────────────────────────────────────────
    // START - Se ejecuta automáticamente cuando arranca la escena
    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Al iniciar, mostramos solo el panel de inicio y ocultamos el resto
        MostrarPanelInicio();

        // Comprobamos que la conexión con la API esté disponible
        // Si no lo está, aparecerá un aviso en la consola de Unity
        if (ApiManager.Instance == null)
            Debug.LogError("[LoginUI] No se encontró el ApiManager. " +
                           "Asegúrate de que el objeto ApiSetup está en la escena anterior (Portada).");
    }


    // =========================================================================
    // NAVEGACIÓN ENTRE PANELES
    // Estas funciones simplemente muestran un panel y ocultan los demás.
    // Conéctalas a los botones desde el Inspector de Unity.
    // =========================================================================

    /// <summary>
    /// Muestra el panel de inicio (los 3 botones principales).
    /// Conéctala al botón "Volver" de los paneles Login y Registro.
    /// </summary>
    public void MostrarPanelInicio()
    {
        panelInicio.SetActive(true);
        panelLogin.SetActive(false);
        panelRegistro.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Limpiamos los errores por si acaso quedaron del intento anterior
        LimpiarErrores();
    }

    /// <summary>
    /// Muestra el panel de inicio de sesión.
    /// Conéctala al botón "Iniciar Sesión" del panel de inicio.
    /// </summary>
    public void MostrarPanelLogin()
    {
        panelInicio.SetActive(false);
        panelLogin.SetActive(true);
        panelRegistro.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LimpiarErrores();
    }

    /// <summary>
    /// Muestra el panel de registro.
    /// Conéctala al botón "Registrarse" del panel de inicio.
    /// </summary>
    public void MostrarPanelRegistro()
    {
        panelInicio.SetActive(false);
        panelLogin.SetActive(false);
        panelRegistro.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LimpiarErrores();
    }

    /// <summary>
    /// Cierra el juego.
    /// Conéctala al botón "Salir" del panel de inicio.
    /// </summary>
    public void Salir()
    {
        Application.Quit();

        // Esta línea solo funciona dentro del editor de Unity (no en el juego final)
        // Es útil para probar que el botón funciona sin tener que cerrar Unity a mano
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    // =========================================================================
    // INICIO DE SESIÓN
    // Se ejecuta cuando el jugador pulsa "Continuar" en el panel de Login.
    // =========================================================================

    /// <summary>
    /// Intenta iniciar sesión con los datos introducidos.
    /// Conéctala al botón "Continuar" del panel Login.
    /// </summary>
    public void OnClickLogin()
    {
        // Recogemos lo que ha escrito el jugador (Trim() elimina espacios al principio y al final)
        string usuario = inputUsuarioLogin.text.Trim();
        string password = inputPasswordLogin.text;

        // Comprobamos que no haya dejado ningún campo vacío
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
        {
            textoErrorLogin.text = "Por favor, introduce tu usuario y contraseña.";
            return; // Salimos sin hacer nada más
        }

        // Ocultamos los paneles mientras esperamos respuesta del servidor
        // para que el jugador sepa que algo está pasando
        MostrarEspera();

        // Llamamos a la API para iniciar sesión.
        // El primer bloque ( _ => ) se ejecuta si TODO va bien.
        // El segundo bloque ( err => ) se ejecuta si algo falla.
        ApiSetup.Auth.Login(usuario, password,
            _ =>
            {
                // Login correcto → ahora cargamos los datos del juego antes de entrar
                CargarDatosYEntrarAlJuego();
            },
            err =>
            {
                // Algo salió mal → volvemos al panel de login y mostramos el error
                MostrarPanelLogin();

                // Dependiendo del tipo de error, mostramos un mensaje u otro
                if (err.Contains("[401]") || err.Contains("[422]"))
                    textoErrorLogin.text = "Usuario o contraseña incorrectos.";
                else
                    textoErrorLogin.text = "Error de conexión. Inténtalo de nuevo.";
            });
    }


    // =========================================================================
    // REGISTRO
    // Se ejecuta cuando el jugador pulsa "Continuar" en el panel de Registro.
    // =========================================================================

    /// <summary>
    /// Intenta crear una cuenta nueva con los datos introducidos.
    /// Conéctala al botón "Continuar" del panel Registro.
    /// </summary>
    public void OnClickRegistrar()
    {
        // Recogemos lo que ha escrito el jugador
        string correo = inputCorreoRegistro.text.Trim();
        string usuario = inputUsuarioRegistro.text.Trim();
        string password = inputPasswordRegistro.text;

        // Comprobamos que no haya dejado ningún campo vacío
        if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
        {
            textoErrorRegistro.text = "Por favor, rellena todos los campos.";
            return; // Salimos sin hacer nada más
        }

        // Ocultamos los paneles mientras esperamos respuesta del servidor
        MostrarEspera();

        // Llamamos a la API para registrar la cuenta nueva
        ApiSetup.Auth.Registrar(usuario, correo, password,
            _ =>
            {
                // Registro correcto → hacemos login automático para no tener que
                // pedirle al jugador que introduzca los datos otra vez
                ApiSetup.Auth.Login(usuario, password,
                    __ =>
                    {
                        // Login automático correcto → cargamos datos y entramos al juego
                        CargarDatosYEntrarAlJuego();
                    },
                    err =>
                    {
                        // El registro fue bien pero el login automático falló
                        // Mandamos al jugador al login para que entre manualmente
                        MostrarPanelLogin();
                        textoErrorLogin.text = "¡Cuenta creada! Ahora inicia sesión con tus datos.";
                    });
            },
            err =>
            {
                // El registro falló → volvemos al panel de registro y mostramos el error
                MostrarPanelRegistro();

                if (err.Contains("[400]") || err.Contains("[409]") || err.Contains("[422]"))
                    textoErrorRegistro.text = "Ese usuario o correo ya está en uso. Prueba con otro.";
                else
                    textoErrorRegistro.text = "Error de conexión. Inténtalo de nuevo.";
            });
    }


    // =========================================================================
    // FUNCIONES AUXILIARES
    // Pequeñas funciones de apoyo usadas por las funciones principales.
    // =========================================================================

    /// <summary>
    /// Oculta todos los paneles para indicar que se está esperando respuesta del servidor.
    /// El jugador verá la pantalla de fondo mientras espera.
    /// Si quieres añadir un panel de "Cargando..." en el futuro, actívalo aquí.
    /// </summary>
    private void MostrarEspera()
    {
        panelInicio.SetActive(false);
        panelLogin.SetActive(false);
        panelRegistro.SetActive(false);
    }

    /// <summary>
    /// Borra los mensajes de error de todos los paneles.
    /// Se llama automáticamente cada vez que se cambia de panel.
    /// </summary>
    private void LimpiarErrores()
    {
        if (textoErrorLogin) textoErrorLogin.text = "";
        if (textoErrorRegistro) textoErrorRegistro.text = "";
    }

    /// <summary>
    /// Una vez que el login o registro es correcto, cargamos los datos del juego
    /// (catálogos de Pokémon, objetos, etc.) y después pasamos al menú principal.
    /// </summary>
    private void CargarDatosYEntrarAlJuego()
    {
        // Comprobamos que el sistema de caché de datos esté disponible
        if (CatalogoCache.Instance == null)
        {
            Debug.LogError("[LoginUI] CatalogoCache no encontrado. Revisa que ApiSetup está bien configurado.");
            MostrarPanelLogin();
            textoErrorLogin.text = "Error interno del juego. Contacta con el equipo.";
            return;
        }

        // Cargamos los catálogos (datos del juego) desde el servidor
        CatalogoCache.Instance.CargarCatalogos(
            () =>
            {
                // Todo cargado correctamente → pasamos a la escena del menú principal
                SceneManager.LoadScene(escenaMenuPrincipal);
            },
            err =>
            {
                // Error al cargar datos → volvemos al login con un mensaje explicativo
                MostrarPanelLogin();
                textoErrorLogin.text = "Error al cargar los datos del juego. Inténtalo de nuevo.";
                Debug.LogError($"[LoginUI] Error cargando catálogos: {err}");
            });
    }
}