using ApiRest.Managers;
using ApiRest.Models;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIOpcionesPanel : MonoBehaviour
{
    // --- Bool estático para que otras escenas sepan si el panel está abierto ---
    public static bool estaAbierto = false;

    // --- Referencias al panel raíz ---
    public GameObject panel;

    // --- Textos del panel izquierdo ---
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoTiempoJugado;
    public TextMeshProUGUI textoFechaCreacion;
    public TextMeshProUGUI textoUltimaConexion;

    // --- Botones del panel derecho ---
    public Button botonPokemons;
    public Button botonMochila;
    public Button botonOpciones;
    public Button botonGuardar;
    public Button botonVolver;
    public Button botonSalir;

    // --- Texto de confirmación de guardado ---
    public GameObject textoGuardadoOk;

    // --- Referencia al Movimiento para devolver el control al cerrar ---
    private Movimiento movimiento;

    // --- Detectar plataforma ---
    private bool esMovil;

    private bool _ignorarInputEsteFrame = false;

    private void Awake()
    {
        ComprobacionInicialParteMovil();

        movimiento = GetComponentInParent<Movimiento>();
        if (movimiento == null)
        {
            Debug.LogError("[UIOpcionesPanel] No se encontro Movimiento en el prefab padre");
        }

        botonPokemons.onClick.AddListener(AbrirPokemons);
        botonMochila.onClick.AddListener(AbrirMochila);
        botonOpciones.onClick.AddListener(AbrirOpciones);
        botonGuardar.onClick.AddListener(GuardarPartida);
        botonVolver.onClick.AddListener(CerrarPanel);
        botonSalir.onClick.AddListener(SalirJuego);

        // El panel empieza cerrado
        panel.SetActive(false);

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }

        AplicarEstiloMenu();
    }

    private void Update()
    {
        // Si acabamos de abrir el panel este frame, ignoramos el input para no cerrarlo al instante
        if (_ignorarInputEsteFrame)
        {
            _ignorarInputEsteFrame = false;
            return;
        }

        if (panel.activeSelf && Input.GetKeyDown(KeyCode.X))
        {
            CerrarPanel();
        }
    }

    // Abre el panel, muestra los datos de la partida y desbloquea el cursor
    public void AbrirPanel()
    {
        _ignorarInputEsteFrame = true;
        estaAbierto = true;
        ActualizarDatosPartida();
        panel.SetActive(true);

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }

        AplicarEstiloMenu();
        // Solo en PC desbloqueamos el cursor para poder pulsar los botones
        if (!esMovil)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Cierra el panel y devuelve el control al jugador
    public void CerrarPanel()
    {
        estaAbierto = false;
        panel.SetActive(false);

        // Solo en PC volvemos a bloquear el cursor
        if (!esMovil)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Avisamos a Movimiento para que vuelva al estado FreeRoam
        if (movimiento != null)
        {
            movimiento.CerrarUIPanel();
        }
    }

    // Rellena los textos con los datos de la partida activa en SessionManager
    private void ActualizarDatosPartida()
    {
        if (SessionManager.Instance == null || !SessionManager.Instance.TienePartida)
        {
            Debug.LogWarning("[UIOpcionesPanel] No hay partida activa para mostrar datos");
            return;
        }

        PartidaCompleta partida = SessionManager.Instance.PartidaActual;
        if (partida == null)
        {
            Debug.LogWarning("[UIOpcionesPanel] La partida activa no tiene datos cargados");
            return;
        }

        if (textoNombre != null)
        {
            textoNombre.text = string.IsNullOrWhiteSpace(partida.nombre) ? "Mi Partida" : partida.nombre;
        }

        if (textoTiempoJugado != null)
        {
            int segundos = SessionManager.Instance.TiempoJugadoSegundos;
            int horas = segundos / 3600;
            int minutos = (segundos % 3600) / 60;
            int segs = segundos % 60;
            textoTiempoJugado.text = $"Tiempo jugado\n{horas}h {minutos}m {segs}s";
        }

        if (textoFechaCreacion != null)
        {
            if (string.IsNullOrWhiteSpace(partida.fecha_creacion))
            {
                textoFechaCreacion.text = "Creada\n-";
            }
            else if (DateTime.TryParse(partida.fecha_creacion, out DateTime fechaCreacion))
            {
                textoFechaCreacion.text = $"Creada\n{fechaCreacion.ToLocalTime():dd/MM/yyyy HH:mm}";
            }
            else
            {
                textoFechaCreacion.text = $"Creada\n{partida.fecha_creacion}";
            }
        }

        if (textoUltimaConexion != null)
        {
            if (string.IsNullOrWhiteSpace(partida.ultima_conexion))
            {
                textoUltimaConexion.text = "Ultima conexion\n-";
            }
            else if (DateTime.TryParse(partida.ultima_conexion, out DateTime ultimaConexion))
            {
                textoUltimaConexion.text = $"Ultima conexion\n{ultimaConexion.ToLocalTime():dd/MM/yyyy HH:mm}";
            }
            else
            {
                textoUltimaConexion.text = $"Ultima conexion\n{partida.ultima_conexion}";
            }
        }
    }

    private void AplicarEstiloMenu()
    {
        if (panel == null) return;

        var panelRect = panel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
            panelRect.localRotation = Quaternion.identity;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        var fondo = panel.GetComponent<Image>();
        if (fondo != null)
            fondo.color = new Color(0.06f, 0.08f, 0.07f, 0.18f);

        EstilizarFondo();
        EstilizarPanelInfo();
        EstilizarPanelBotones();
        EstilizarTextoGuardado();
    }

    private void EstilizarFondo()
    {
        var panelFondo = BuscarHijo(panel.transform, "PanelFondo") as RectTransform;
        if (panelFondo == null) return;

        panelFondo.localScale = Vector3.one;
        panelFondo.localRotation = Quaternion.identity;
        panelFondo.anchorMin = Vector2.zero;
        panelFondo.anchorMax = Vector2.one;
        panelFondo.offsetMin = Vector2.zero;
        panelFondo.offsetMax = Vector2.zero;

        var imagen = panelFondo.GetComponent<Image>();
        if (imagen != null)
            imagen.color = new Color(0.06f, 0.08f, 0.07f, 0.18f);
    }

    private void EstilizarPanelInfo()
    {
        var panelIzquierda = BuscarHijo(panel.transform, "PanelIzquierda") as RectTransform;
        if (panelIzquierda == null) return;

        panelIzquierda.localScale = Vector3.one;
        panelIzquierda.localRotation = Quaternion.identity;
        panelIzquierda.anchorMin = new Vector2(0.035f, 0.12f);
        panelIzquierda.anchorMax = new Vector2(0.33f, 0.88f);
        panelIzquierda.offsetMin = Vector2.zero;
        panelIzquierda.offsetMax = Vector2.zero;

        var imagen = panelIzquierda.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.color = new Color(0.96f, 0.98f, 0.93f, 0.96f);
            AsegurarBorde(imagen, new Color(0.05f, 0.22f, 0.15f, 1f), new Vector2(3f, -3f));
        }

        EstilizarTexto(textoNombre, 17f, TextAlignmentOptions.TopLeft, new Color(0.05f, 0.09f, 0.08f));
        EstilizarTexto(textoTiempoJugado, 20f, TextAlignmentOptions.TopLeft, new Color(0.08f, 0.12f, 0.11f));
        EstilizarTexto(textoFechaCreacion, 17f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.16f, 0.15f));
        EstilizarTexto(textoUltimaConexion, 17f, TextAlignmentOptions.TopLeft, new Color(0.12f, 0.16f, 0.15f));

        if (textoNombre != null)
        {
            textoNombre.textWrappingMode = TextWrappingModes.NoWrap;
            textoNombre.characterSpacing = 0f;
            textoNombre.fontSizeMin = 11f;
            textoNombre.fontSizeMax = 17f;
        }

        ColocarTexto(textoNombre, new Vector2(0.35f, 0.80f), new Vector2(0.96f, 0.96f));
        ColocarTexto(textoTiempoJugado, new Vector2(0.08f, 0.54f), new Vector2(0.92f, 0.70f));
        ColocarTexto(textoFechaCreacion, new Vector2(0.08f, 0.32f), new Vector2(0.92f, 0.48f));
        ColocarTexto(textoUltimaConexion, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.28f));

        var cajaPokeball = BuscarHijo(panelIzquierda, "CajaPokeball") as RectTransform;
        if (cajaPokeball != null)
        {
            cajaPokeball.anchorMin = new Vector2(0.07f, 0.80f);
            cajaPokeball.anchorMax = new Vector2(0.28f, 0.94f);
            cajaPokeball.offsetMin = Vector2.zero;
            cajaPokeball.offsetMax = Vector2.zero;

            var fondoCaja = cajaPokeball.GetComponent<Image>();
            if (fondoCaja != null)
            {
                fondoCaja.color = Color.clear;
                fondoCaja.enabled = false;
                fondoCaja.raycastTarget = false;
            }
        }
    }

    private void EstilizarPanelBotones()
    {
        var panelDerecha = BuscarHijo(panel.transform, "PanelDerecha") as RectTransform;
        if (panelDerecha == null) return;

        panelDerecha.localScale = Vector3.one;
        panelDerecha.localRotation = Quaternion.identity;
        panelDerecha.anchorMin = new Vector2(0.80f, 0.13f);
        panelDerecha.anchorMax = new Vector2(0.965f, 0.87f);
        panelDerecha.offsetMin = Vector2.zero;
        panelDerecha.offsetMax = Vector2.zero;

        var imagen = panelDerecha.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.color = new Color(0.96f, 0.98f, 0.93f, 0.96f);
            AsegurarBorde(imagen, new Color(0.05f, 0.22f, 0.15f, 1f), new Vector2(3f, -3f));
        }

        Button[] botones =
        {
            botonPokemons, botonMochila, botonOpciones,
            botonGuardar, botonVolver, botonSalir
        };

        for (int i = 0; i < botones.Length; i++)
        {
            EstilizarBoton(botones[i], i, botones.Length);
        }
    }

    private void EstilizarBoton(Button boton, int indice, int total)
    {
        if (boton == null) return;

        var rect = boton.GetComponent<RectTransform>();
        if (rect != null)
        {
            float alto = 1f / total;
            float arriba = 1f - indice * alto;
            float abajo = arriba - alto;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(0.08f, abajo + 0.012f);
            rect.anchorMax = new Vector2(0.92f, arriba - 0.012f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Color colorBase = indice == total - 1
            ? new Color(0.62f, 0.18f, 0.15f, 0.92f)
            : new Color(0.05f, 0.35f, 0.19f, 0.92f);

        var imagen = boton.GetComponent<Image>();
        if (imagen != null)
        {
            imagen.color = colorBase;
            AsegurarBorde(imagen, new Color(0.02f, 0.16f, 0.09f, 1f), new Vector2(2f, -2f));
        }

        var colores = boton.colors;
        colores.normalColor = colorBase;
        colores.highlightedColor = indice == total - 1
            ? new Color(0.78f, 0.26f, 0.20f, 1f)
            : new Color(0.08f, 0.46f, 0.25f, 1f);
        colores.pressedColor = indice == total - 1
            ? new Color(0.45f, 0.10f, 0.08f, 1f)
            : new Color(0.03f, 0.24f, 0.13f, 1f);
        colores.selectedColor = colores.highlightedColor;
        colores.colorMultiplier = 1f;
        boton.colors = colores;

        var texto = boton.GetComponentInChildren<TextMeshProUGUI>();
        if (texto != null)
        {
            texto.fontSize = 21f;
            texto.enableAutoSizing = true;
            texto.fontSizeMin = 15f;
            texto.fontSizeMax = 21f;
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            texto.overflowMode = TextOverflowModes.Ellipsis;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = Color.white;
        }
    }

    private void EstilizarTextoGuardado()
    {
        if (textoGuardadoOk == null) return;

        var texto = textoGuardadoOk.GetComponent<TextMeshProUGUI>();
        if (texto != null)
        {
            texto.text = "Partida guardada";
            texto.fontSize = 24f;
            texto.color = new Color(0.94f, 1f, 0.78f);
            texto.alignment = TextAlignmentOptions.Center;
        }

        var rect = textoGuardadoOk.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.36f, 0.07f);
            rect.anchorMax = new Vector2(0.78f, 0.16f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private void EstilizarTexto(TextMeshProUGUI texto, float size, TextAlignmentOptions alineacion, Color color)
    {
        if (texto == null) return;

        texto.fontSize = size;
        texto.enableAutoSizing = true;
        texto.fontSizeMin = Mathf.Max(12f, size - 6f);
        texto.fontSizeMax = size;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.overflowMode = TextOverflowModes.Ellipsis;
        texto.alignment = alineacion;
        texto.color = color;
        texto.fontStyle = FontStyles.Bold;
        texto.characterSpacing = 1.5f;
        texto.lineSpacing = 3f;
    }

    private void AsegurarBorde(Graphic graphic, Color color, Vector2 distancia)
    {
        if (graphic == null) return;

        var outline = graphic.GetComponent<Outline>();
        if (outline == null)
            outline = graphic.gameObject.AddComponent<Outline>();

        outline.effectColor = color;
        outline.effectDistance = distancia;
        outline.useGraphicAlpha = true;
    }

    private void ColocarTexto(TextMeshProUGUI texto, Vector2 min, Vector2 max)
    {
        if (texto == null) return;

        var rect = texto.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private Transform BuscarHijo(Transform raiz, string nombre)
    {
        if (raiz == null) return null;

        foreach (Transform hijo in raiz)
        {
            if (hijo.name == nombre)
                return hijo;

            Transform encontrado = BuscarHijo(hijo, nombre);
            if (encontrado != null)
                return encontrado;
        }

        return null;
    }

    // Abre la pantalla de equipo pokemon
    private void AbrirPokemons()
    {
        CerrarPanel();
        if (movimiento != null)
        {
            movimiento.AbrirCentroIbermon();
        }
    }

    // Abre el inventario
    private void AbrirMochila()
    {
        CerrarPanel();
        if (movimiento != null)
        {
            movimiento.AbrirMochila();
        }
    }
    private void SalirJuego()
    {
        Application.Quit();
    }

    // Carga la escena Opciones igual que antes, guardando posición y escena actuales
    private void AbrirOpciones()
    {
        GuardarPosicionAnterior.escenaAnterior = SceneManager.GetActiveScene().name;
        GuardarPosicionAnterior.posicionAnterior = ObtenerPosicionJugador();
        OcultarPanelSinCambiarEstado();

        if (movimiento != null)
        {
            movimiento.CerrarUIPanel();
        }

        // Deshabilitar AudioListener del Player ANTES de cargar la escena
        var listener = movimiento != null
            ? movimiento.GetComponentInChildren<AudioListener>()
            : GetComponentInParent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }

        SceneManager.LoadSceneAsync("Opciones");
    }

    private Vector3 ObtenerPosicionJugador()
    {
        if (movimiento != null)
        {
            return movimiento.transform.position;
        }

        return transform.root.position;
    }

    private void OcultarPanelSinCambiarEstado()
    {
        estaAbierto = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Envía la posición y tiempo actuales del jugador a la API
    private void GuardarPartida()
    {
        if (SessionManager.Instance == null || !SessionManager.Instance.TienePartida)
        {
            Debug.LogWarning("[UIOpcionesPanel] No hay partida activa, no se puede guardar");
            return;
        }

        string escena = SceneManager.GetActiveScene().name;
        Vector3 posicionJugador = ObtenerPosicionJugador();
        float x = posicionJugador.x;
        float y = posicionJugador.y;
        string partidaId = SessionManager.Instance.PartidaId;
        int tiempoJugado = SessionManager.Instance.TiempoJugadoSegundos;
        string ultimaConexion = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        ApiSetup.Partida.ActualizarPosicion(partidaId, escena, x, y, tiempoJugado, ultimaConexion,
            ManejarGuardadoExitoso, ManejarGuardadoFallido);
    }

    // Se ejecuta cuando la API confirma que se guardó correctamente
    private void ManejarGuardadoExitoso(PartidaCompleta partidaActualizada)
    {
        if (SessionManager.Instance != null)
        {
            SessionManager.Instance.ActualizarPartidaActual(partidaActualizada);
        }

        Debug.Log("[UIOpcionesPanel] Partida guardada correctamente");

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(true);
            StartCoroutine(OcultarTextoGuardado());
        }
    }

    // Oculta el texto de confirmación tras 4 segundos
    private IEnumerator OcultarTextoGuardado()
    {
        yield return new WaitForSeconds(1f);

        if (textoGuardadoOk != null)
        {
            textoGuardadoOk.SetActive(false);
        }
    }

    // Se ejecuta si la API devuelve error al guardar
    private void ManejarGuardadoFallido(string mensajeError)
    {
        Debug.LogError($"[UIOpcionesPanel] Error al guardar partida: {mensajeError}");
    }

    private void ComprobacionInicialParteMovil()
    {
#if UNITY_ANDROID || UNITY_IOS
        esMovil = true;
#else
        esMovil = false;
#endif
    }
}
