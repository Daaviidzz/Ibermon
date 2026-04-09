using ApiRest.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controlador de la escena de Login.
/// Gestiona los paneles de login y registro, y la transición al menú principal.
///
/// REQUISITOS en Unity:
///   - Escena "Login" con un Canvas que contenga:
///     • PanelLogin: InputField usuario, InputField password, Button login,
///                   Button "¿No tienes cuenta?" (ir a registro), Text error.
///     • PanelRegistro: InputField usuario, InputField email, InputField password,
///                      Button registrar, Button "Volver a login", Text error.
///     • PanelCargando: Text con mensaje de estado.
///   - Este script en un GameObject de la escena "Login".
///   - ApiSetup en la escena Portada (o Login) - se propaga con DontDestroyOnLoad.
/// </summary>
public class LoginUI : MonoBehaviour
{
    // ─── Panel Login ──────────────────────────────────────────────────────────
    [Header("Panel Login")]
    public GameObject        panelLogin;
    public TMP_InputField    inputUsuarioLogin;
    public TMP_InputField    inputPasswordLogin;
    public Button            botonLogin;
    public Button            botonIrRegistro;
    public TextMeshProUGUI   textoErrorLogin;

    // ─── Panel Registro ───────────────────────────────────────────────────────
    [Header("Panel Registro")]
    public GameObject        panelRegistro;
    public TMP_InputField    inputUsuarioRegistro;
    public TMP_InputField    inputEmailRegistro;
    public TMP_InputField    inputPasswordRegistro;
    public Button            botonRegistrar;
    public Button            botonIrLogin;
    public TextMeshProUGUI   textoErrorRegistro;

    // ─── Panel Cargando ───────────────────────────────────────────────────────
    [Header("Panel Cargando")]
    public GameObject        panelCargando;
    public TextMeshProUGUI   textoCargando;

    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Configuración")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string escenaMenuPrincipal = "MenuPrincipal";

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        MostrarLogin();

        // Validar que ApiSetup esté disponible
        if (ApiManager.Instance == null)
            Debug.LogError("[LoginUI] ApiManager no encontrado. " +
                           "Asegúrate de que ApiSetup está en la escena Portada (o Login).");
    }

    // ─── Login ────────────────────────────────────────────────────────────────

    public void MostrarLogin()
    {
        panelLogin.SetActive(true);
        panelRegistro.SetActive(false);
        panelCargando.SetActive(false);
        if (textoErrorLogin) textoErrorLogin.text = "";
    }

    /// <summary>Conectado al botón "Iniciar sesión".</summary>
    public void OnClickLogin()
    {
        string usuario  = inputUsuarioLogin.text.Trim();
        string password = inputPasswordLogin.text;

        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
        {
            textoErrorLogin.text = "Introduce usuario y contraseña.";
            return;
        }

        SetCargando("Iniciando sesión...");

        ApiSetup.Auth.Login(usuario, password,
            _ =>
            {
                SetCargando("Cargando catálogos...");
                CargarCatalogosYContinuar();
            },
            err =>
            {
                MostrarLogin();
                textoErrorLogin.text = err.Contains("[401]") || err.Contains("[422]")
                    ? "Usuario o contraseña incorrectos."
                    : $"Error de conexión: {err}";
            });
    }

    // ─── Registro ─────────────────────────────────────────────────────────────

    public void MostrarRegistro()
    {
        panelLogin.SetActive(false);
        panelRegistro.SetActive(true);
        panelCargando.SetActive(false);
        if (textoErrorRegistro) textoErrorRegistro.text = "";
    }

    /// <summary>Conectado al botón "Crear cuenta".</summary>
    public void OnClickRegistrar()
    {
        string usuario  = inputUsuarioRegistro.text.Trim();
        string email    = inputEmailRegistro.text.Trim();
        string password = inputPasswordRegistro.text;

        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            textoErrorRegistro.text = "Rellena todos los campos.";
            return;
        }

        SetCargando("Registrando cuenta...");

        ApiSetup.Auth.Registrar(usuario, email, password,
            _ =>
            {
                // Auto-login tras registro exitoso
                SetCargando("Iniciando sesión...");
                ApiSetup.Auth.Login(usuario, password,
                    __ =>
                    {
                        SetCargando("Cargando catálogos...");
                        CargarCatalogosYContinuar();
                    },
                    err =>
                    {
                        MostrarLogin();
                        textoErrorLogin.text = "Cuenta creada. Inicia sesión.";
                    });
            },
            err =>
            {
                MostrarRegistro();
                textoErrorRegistro.text = err.Contains("[400]") || err.Contains("[409]") || err.Contains("[422]")
                    ? "El usuario o email ya está en uso."
                    : $"Error: {err}";
            });
    }

    // ─── Post-login ───────────────────────────────────────────────────────────

    private void CargarCatalogosYContinuar()
    {
        if (CatalogoCache.Instance == null)
        {
            Debug.LogError("[LoginUI] CatalogoCache.Instance es null. " +
                           "Asegúrate de que ApiSetup está correctamente configurado.");
            MostrarLogin();
            textoErrorLogin.text = "Error interno. Revisa la consola.";
            return;
        }

        CatalogoCache.Instance.CargarCatalogos(
            onDone: () => SceneManager.LoadScene(escenaMenuPrincipal),
            onError: err =>
            {
                MostrarLogin();
                textoErrorLogin.text = $"Error cargando datos del juego: {err}";
                Debug.LogError($"[LoginUI] Error catálogos: {err}");
            });
    }

    private void SetCargando(string mensaje)
    {
        panelLogin.SetActive(false);
        panelRegistro.SetActive(false);
        panelCargando.SetActive(true);
        if (textoCargando) textoCargando.text = mensaje;
    }
}
