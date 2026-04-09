using System;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente para el prefab de cada entrada de partida en la lista de partidas.
///
/// REQUISITOS del prefab "PartidaEntry":
///   - Un Button (el fondo clicable)
///   - TextMeshProUGUI textoPersonaje  (ej: "Jugador: Chico")
///   - TextMeshProUGUI textoMapa       (ej: "Ubicación: PuebloFuenlabrada")
///   - TextMeshProUGUI textoMedallas   (ej: "Medallas: 0")
///   - TextMeshProUGUI textoTiempo     (ej: "Tiempo: 00:12:34")
///   - (Opcional) Button botonEliminar con texto "X"
/// </summary>
public class PartidaEntryUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI textoPersonaje;
    public TextMeshProUGUI textoMapa;
    public TextMeshProUGUI textoMedallas;
    public TextMeshProUGUI textoTiempo;
    public Button          botonEliminar;

    private PartidaResumen        _resumen;
    private Action<PartidaResumen> _onSeleccionar;
    private Action<PartidaResumen> _onEliminar;

    /// <summary>Inicializa la entrada con los datos de la partida y los callbacks.</summary>
    public void Inicializar(
        PartidaResumen        resumen,
        Action<PartidaResumen> onSeleccionar,
        Action<PartidaResumen> onEliminar = null)
    {
        _resumen       = resumen;
        _onSeleccionar = onSeleccionar;
        _onEliminar    = onEliminar;

        // Rellenar textos
        if (textoPersonaje) textoPersonaje.text = $"Jugador: {CapitalizarPrimera(resumen.personaje_elegido)}";
        if (textoMapa)      textoMapa.text      = $"Ubicación: {resumen.mapa_actual}";
        if (textoMedallas)  textoMedallas.text  = $"Medallas: {resumen.medallas?.Count ?? 0}";
        if (textoTiempo)    textoTiempo.text     = $"Tiempo: {FormatearTiempo(resumen.tiempo_jugado)}";

        // Ocultar botón eliminar si no hay callback
        if (botonEliminar) botonEliminar.gameObject.SetActive(onEliminar != null);
    }

    /// <summary>Conectado al Button principal del prefab (en Inspector).</summary>
    public void OnClickSeleccionar()
    {
        _onSeleccionar?.Invoke(_resumen);
    }

    /// <summary>Conectado al botonEliminar (en Inspector).</summary>
    public void OnClickEliminar()
    {
        _onEliminar?.Invoke(_resumen);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static string FormatearTiempo(int segundos)
    {
        int h = segundos / 3600;
        int m = (segundos % 3600) / 60;
        int s = segundos % 60;
        return $"{h:00}:{m:00}:{s:00}";
    }

    private static string CapitalizarPrimera(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        return char.ToUpper(texto[0]) + texto.Substring(1);
    }
}
