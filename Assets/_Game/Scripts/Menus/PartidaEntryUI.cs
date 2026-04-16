using System;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Componente del prefab de cada fila en la lista de partidas.
//
// Estructura mínima del prefab:
//   - Button (el fondo, todo el prefab es clicable)
//       - TextoNombre   (TextMeshProUGUI) → "Partida 1"
//       - TextoFecha    (TextMeshProUGUI) → "Última vez: 16/04/2026"
//       - BotonEliminar (Button)          → la X para borrar
public class PartidaEntryUI : MonoBehaviour
{
    [Header("UI del prefab")]
    public TextMeshProUGUI textoNombre;   // Mostrará "Partida 1", "Partida 2"...
    public TextMeshProUGUI textoFecha;    // Mostrará la fecha de última modificación
    public Button botonEliminar; // El botón X para borrar

    // Guardamos los callbacks para usarlos al pulsar
    private Action<PartidaResumen> _onSeleccionar;
    private Action<PartidaResumen> _onEliminar;
    private PartidaResumen _datos;

    // MenuPartidas llama a este método nada más instanciar el prefab
    public void Inicializar(
        PartidaResumen partida,
        Action<PartidaResumen> onSeleccionar,
        Action<PartidaResumen> onEliminar)
    {
        _datos = partida;
        _onSeleccionar = onSeleccionar;
        _onEliminar = onEliminar;

        int numero = transform.GetSiblingIndex() + 1;
        textoNombre.text = $"Partida {numero}";
        textoFecha.text = $"Veces jugada: {partida.tiempo_jugado}";

        // Mostrar el botón de eliminar (si no quisiéramos mostrarlo en algún caso, aquí lo ocultaríamos)
        botonEliminar.gameObject.SetActive(true);
    }

    // Conecta este método al OnClick del Button principal del prefab (el fondo)
    public void OnClickSeleccionar()
    {
        _onSeleccionar?.Invoke(_datos);
    }

    // Conecta este método al OnClick del BotonEliminar
    public void OnClickEliminar()
    {
        _onEliminar?.Invoke(_datos);
    }
}