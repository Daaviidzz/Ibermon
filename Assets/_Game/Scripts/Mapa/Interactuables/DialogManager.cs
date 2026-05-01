using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    [SerializeField] private ControladorTextosUI controladorTextosUI;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowDialogText(string text)
    {
        Debug.Log("Intentando mostrar texto: " + text);
        controladorTextosUI.activarDesactivarCajaDeTextos(true);
        controladorTextosUI.mostrarTextos(text);

        yield return null;

        Debug.Log("Esperando entrada del jugador...");
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
        Debug.Log("Tecla pulsada, cerrando cuadro.");

        controladorTextosUI.activarDesactivarCajaDeTextos(false);
    }
}

