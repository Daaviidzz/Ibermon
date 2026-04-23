using System;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Componente del prefab de cada fila en la lista de partidas.
//
// Estructura minima del prefab:
//   - Button (el fondo, todo el prefab es clicable)
//       - TextoNombre   (TextMeshProUGUI) → nombre de la partida
//       - TextoTiempo   (TextMeshProUGUI) → tiempo jugado formateado
//       - BotonEliminar (Button)          → la X para borrar
public class PartidaEntryUI : MonoBehaviour
{
    [Header("UI del prefab")]
    public TextMeshProUGUI textoNombre;  // Mostrara el nombre de la partida
    public TextMeshProUGUI textoTiempo; // Mostrara el tiempo jugado formateado
    public Button botonEliminar;        // El boton X para borrar

    // Guardamos los callbacks para usarlos al pulsar
    private Action<PartidaResumen> _onSeleccionar;
    private Action<PartidaResumen> _onEliminar;
    private PartidaResumen _datos;

    // MenuPartidas llama a este metodo nada mas instanciar el prefab
    public void Inicializar(
        PartidaResumen partida,
        Action<PartidaResumen> onSeleccionar,
        Action<PartidaResumen> onEliminar)
    {
        _datos = partida;
        _onSeleccionar = onSeleccionar;
        _onEliminar = onEliminar;

        textoNombre.text = partida.nombre;
        textoTiempo.text = FormatearTiempo(partida.tiempo_jugado);

        // Mostrar el boton de eliminar (si no quisieramos mostrarlo en algun caso, aqui lo ocultariamos)
        botonEliminar.gameObject.SetActive(true);
    }

    // Formatea los segundos en horas y minutos para mostrar al jugador
    private string FormatearTiempo(int segundos)
    {
        int horas = segundos / 3600;
        int minutos = (segundos % 3600) / 60;
        return $"{horas}h {minutos}m jugadas";
    }

    // Conecta este metodo al OnClick del Button principal del prefab (el fondo)
    public void OnClickSeleccionar() => _onSeleccionar?.Invoke(_datos);

    // Conecta este metodo al OnClick del BotonEliminar
    public void OnClickEliminar() => _onEliminar?.Invoke(_datos);
}