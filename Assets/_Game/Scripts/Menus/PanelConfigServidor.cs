using ApiRest.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PanelConfigServidor : MonoBehaviour
{
    [Header("Campos")]
    [SerializeField] private TMP_InputField campoUrlServidor;
    [SerializeField] private TextMeshProUGUI textoEstado;

    [Header("Botones")]
    [SerializeField] private Button botonGuardar;
    [SerializeField] private Button botonProbar;
    [SerializeField] private Button botonCerrar;

    private void OnEnable()
    {
        CargarUrlGuardada();
    }

    public void Abrir()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        CargarUrlGuardada();
        SeleccionarCampoUrl();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Cerrar()
    {
        gameObject.SetActive(false);
    }

    public bool EstaAbierto()
    {
        return gameObject.activeInHierarchy;
    }

    private void CargarUrlGuardada()
    {
        if (campoUrlServidor == null)
        {
            return;
        }

        string urlGuardada = PlayerPrefs.GetString(ApiSetup.PrefKeyUrlServidor, string.Empty);
        campoUrlServidor.text = urlGuardada;

        if (textoEstado != null)
        {
            textoEstado.text = string.Empty;
        }
    }

    private void SeleccionarCampoUrl()
    {
        if (campoUrlServidor == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(campoUrlServidor.gameObject);
        campoUrlServidor.ActivateInputField();
        campoUrlServidor.MoveTextEnd(false);
    }

    public void GuardarUrl()
    {
        string url = ObtenerUrlCampo();
        if (string.IsNullOrWhiteSpace(url))
        {
            MostrarEstado("Escribe una URL antes de guardar.");
            return;
        }

        PlayerPrefs.SetString(ApiSetup.PrefKeyUrlServidor, url);
        PlayerPrefs.Save();
        ApiSetup.AplicarUrlServidor(url);
        MostrarEstado("URL guardada y aplicada.");
        Debug.Log("[PanelConfigServidor] URL guardada: " + url);
    }

    public void ProbarConexion()
    {
        string url = ObtenerUrlCampo();
        if (string.IsNullOrWhiteSpace(url))
        {
            MostrarEstado("Escribe una URL antes de probar.");
            return;
        }

        StartCoroutine(ProbarConexionRutina(url));
    }

    private System.Collections.IEnumerator ProbarConexionRutina(string urlServidor)
    {
        MostrarEstado("Probando conexion...");
        string urlPrueba = urlServidor.TrimEnd('/') + "/catalogo/items";

        using (UnityWebRequest peticion = UnityWebRequest.Get(urlPrueba))
        {
            yield return peticion.SendWebRequest();

            if (peticion.result == UnityWebRequest.Result.Success)
            {
                MostrarEstado("Conexion correcta.");
                Debug.Log("[PanelConfigServidor] Conexion correcta: " + urlPrueba);
            }
            else
            {
                MostrarEstado("Error de conexion: " + peticion.error);
                Debug.LogWarning("[PanelConfigServidor] Error de conexion: " + peticion.error);
            }
        }
    }

    private string ObtenerUrlCampo()
    {
        if (campoUrlServidor == null)
        {
            return string.Empty;
        }

        return campoUrlServidor.text.Trim();
    }

    private void MostrarEstado(string mensaje)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }
}
