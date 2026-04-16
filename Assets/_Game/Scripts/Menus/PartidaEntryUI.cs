using System;
using ApiRest.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartidaEntryUI : MonoBehaviour
{
    [Header("UI del prefab")]
    public TextMeshProUGUI textoNombre;   // Mostrará "Partida 1", "Partida 2"...
    public TextMeshProUGUI textoFecha;    // Mostrará la fecha de última modificación
    public Button botonEliminar; // El botón X para borrar

    private Action<PartidaResumen> _onSeleccionar;
    private Action<PartidaResumen> _onEliminar;
    private PartidaResumen _datos;

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

        botonEliminar.gameObject.SetActive(true);
    }

    public void OnClickSeleccionar()
    {
        _onSeleccionar?.Invoke(_datos);
    }

    public void OnClickEliminar()
    {
        _onEliminar?.Invoke(_datos);
    }
}