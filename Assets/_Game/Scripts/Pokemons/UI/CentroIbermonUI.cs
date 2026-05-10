using System.Collections.Generic;
using ApiRest.Managers;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CentroIbermonUI : MonoBehaviour
{
    [Header("Refs UI")]
    [SerializeField] Transform equipoContainer;
    [SerializeField] Transform centroContainer;
    [SerializeField] CentroIbermonSlotUI slotPrefab;
    [SerializeField] Button botonVolver;
    [SerializeField] TextMeshProUGUI mensajeText;

    private readonly List<IbermonJugador> _equipo = new();
    private readonly List<IbermonJugador> _centro = new();
    private CentroIbermonSlotUI _seleccionado;
    private bool _cargando;
    private int _ignorarInputHastaFrame;

    private const string LadoEquipo = "equipo";
    private const string LadoCentro = "centro";
    private const int MaxEquipo = 6;

    private void Awake()
    {
        if (botonVolver != null)
            botonVolver.onClick.AddListener(Cerrar);
    }

    public void Abrir()
    {
        AsegurarEventSystem();
        _ignorarInputHastaFrame = Time.frameCount + 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.SetActive(true);
        LimpiarMensaje();
        CargarListas();
    }

    private void Update()
    {
        if (Time.frameCount <= _ignorarInputHastaFrame) return;

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            Cerrar();
    }

    public void Cerrar()
    {
        _cargando = false;
        LimpiarSeleccion();
        gameObject.SetActive(false);

        var movimiento = GameObject.FindWithTag("Player")?.GetComponent<Movimiento>();
        if (movimiento != null)
            movimiento.CerrarUIPanel();
    }

    private void CargarListas()
    {
        if (_cargando) return;
        _cargando = true;

        string partidaId = SessionManager.Instance?.PartidaId;
        if (string.IsNullOrEmpty(partidaId))
        {
            _cargando = false;
            MostrarMensaje("No hay partida activa.");
            Debug.LogError("[CentroIbermonUI] No hay partida activa.");
            return;
        }

        bool equipoRecibido = false;
        bool centroRecibido = false;
        bool yaFinalizado = false;
        List<IbermonJugador> equipoTmp = null;
        List<IbermonJugador> centroTmp = null;

        void IntentarFinalizar()
        {
            if (yaFinalizado || !equipoRecibido || !centroRecibido) return;

            yaFinalizado = true;
            _cargando = false;
            _equipo.Clear();
            _centro.Clear();
            _equipo.AddRange(equipoTmp ?? new List<IbermonJugador>());
            _centro.AddRange(centroTmp ?? new List<IbermonJugador>());
            SessionManager.Instance?.SetEquipoAPI(_equipo);
            Renderizar();
        }

        void ReportarError(string error)
        {
            if (yaFinalizado) return;

            yaFinalizado = true;
            _cargando = false;
            MostrarMensaje("No se pudieron cargar los Ibermon.");
            Debug.LogError($"[CentroIbermonUI] {error}");
            Renderizar();
        }

        ApiSetup.IbermonJugador.ObtenerEquipo(partidaId,
            lista => { equipoTmp = lista; equipoRecibido = true; IntentarFinalizar(); },
            err => ReportarError($"Error cargando equipo: {err}"));

        ApiSetup.IbermonJugador.ObtenerCentro(partidaId,
            lista => { centroTmp = lista; centroRecibido = true; IntentarFinalizar(); },
            err => ReportarError($"Error cargando centro: {err}"));
    }

    private void Renderizar()
    {
        LimpiarSeleccion();
        LimpiarContainer(equipoContainer);
        LimpiarContainer(centroContainer);

        if (slotPrefab == null)
        {
            Debug.LogError("[CentroIbermonUI] slotPrefab no asignado.");
            return;
        }

        for (int i = 0; i < MaxEquipo; i++)
        {
            IbermonJugador ibermon = i < _equipo.Count ? _equipo[i] : null;
            CrearSlot(equipoContainer, ibermon, LadoEquipo);
        }

        foreach (IbermonJugador ibermon in _centro)
            CrearSlot(centroContainer, ibermon, LadoCentro);

        CrearSlot(centroContainer, null, LadoCentro);
    }

    private void CrearSlot(Transform container, IbermonJugador ibermon, string lado)
    {
        if (container == null) return;

        CentroIbermonSlotUI slot = Instantiate(slotPrefab, container);
        slot.gameObject.SetActive(true);
        slot.Configurar(ibermon, lado, this);
    }

    public void OnSlotClick(CentroIbermonSlotUI slot)
    {
        if (_cargando || slot == null) return;

        if (_seleccionado == null)
        {
            if (!slot.TieneIbermon)
            {
                MostrarMensaje("Elige primero un Ibermon.");
                return;
            }

            _seleccionado = slot;
            slot.SetResaltado(true);
            LimpiarMensaje();
            return;
        }

        if (_seleccionado == slot)
        {
            LimpiarSeleccion();
            return;
        }

        IntentarMover(_seleccionado, slot);
    }

    private void IntentarMover(CentroIbermonSlotUI origen, CentroIbermonSlotUI destino)
    {
        if (origen.Lado == destino.Lado && destino.TieneIbermon)
        {
            origen.SetResaltado(false);
            _seleccionado = destino;
            destino.SetResaltado(true);
            return;
        }

        if (origen.Lado == destino.Lado && !destino.TieneIbermon)
        {
            LimpiarSeleccion();
            return;
        }

        if (origen.Lado == LadoEquipo && !destino.TieneIbermon && _equipo.Count <= 1)
        {
            MostrarMensaje("No puedes dejar el equipo vacio.");
            LimpiarSeleccion();
            return;
        }

        if (destino.Lado == LadoEquipo && !destino.TieneIbermon && _equipo.Count >= MaxEquipo)
        {
            MostrarMensaje("El equipo ya tiene 6 Ibermon.");
            LimpiarSeleccion();
            return;
        }

        EjecutarMovimiento(origen, destino);
    }

    private void EjecutarMovimiento(CentroIbermonSlotUI origen, CentroIbermonSlotUI destino)
    {
        string partidaId = SessionManager.Instance?.PartidaId;
        if (string.IsNullOrEmpty(partidaId)) return;

        _cargando = true;
        MostrarMensaje("Moviendo...");

        ApiSetup.IbermonJugador.MoverIbermon(partidaId, origen.IbermonId, destino.Lado,
            _ =>
            {
                if (destino.TieneIbermon)
                    MoverDestinoDelSwap(partidaId, destino, origen.Lado);
                else
                    FinalizarMovimiento();
            },
            ManejarErrorMovimiento);
    }

    private void MoverDestinoDelSwap(string partidaId, CentroIbermonSlotUI destino, string ladoOrigen)
    {
        ApiSetup.IbermonJugador.MoverIbermon(partidaId, destino.IbermonId, ladoOrigen,
            _ => FinalizarMovimiento(),
            ManejarErrorMovimiento);
    }

    private void FinalizarMovimiento()
    {
        _cargando = false;
        LimpiarSeleccion();
        LimpiarMensaje();
        CargarListas();
    }

    private void ManejarErrorMovimiento(string error)
    {
        _cargando = false;
        MostrarMensaje("No se pudo mover el Ibermon.");
        Debug.LogError($"[CentroIbermonUI] {error}");
        LimpiarSeleccion();
        CargarListas();
    }

    private void LimpiarSeleccion()
    {
        if (_seleccionado != null)
            _seleccionado.SetResaltado(false);

        _seleccionado = null;
    }

    private void LimpiarContainer(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
            Destroy(container.GetChild(i).gameObject);
    }

    private void MostrarMensaje(string mensaje)
    {
        if (mensajeText != null)
            mensajeText.text = mensaje;
        else
            Debug.Log(mensaje);
    }

    private void LimpiarMensaje()
    {
        if (mensajeText != null)
            mensajeText.text = "";
    }

    private static void AsegurarEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
