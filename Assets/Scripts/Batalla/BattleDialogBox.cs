using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogBox : MonoBehaviour
{
    // --- REFERENCIAS DE UI Y CONFIGURACIÓN ---

    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;
    [SerializeField] TextMeshProUGUI dialogText;

    // Paneles contenedores
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    // Listas de textos
    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;

    // Textos de detalle (PP y Tipo)
    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    // --- MÉTODOS PÚBLICOS ---

    public void SetDialog(string dialog) => dialogText.text = dialog;

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";

        // OPTIMIZACIÓN: Cacheamos el tiempo de espera para no crear "basura" (Garbage) en cada vuelta del bucle.
        var waitTime = new WaitForSeconds(1f / lettersPerSecond);

        foreach (var letter in dialog)
        {
            dialogText.text += letter;
            yield return waitTime;
        }

        yield return new WaitForSeconds(1f);
    }

    // Métodos "One-liner" usando Expression Body (=>) para mayor limpieza
    public void EnableDialogText(bool enabled) => dialogText.enabled = enabled;
    public void EnableActionSelector(bool enabled) => actionSelector.SetActive(enabled);

    public void EnableMoveSelector(bool enabled) // Corregido typo: Eneable -> Enable
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            // Usamos operador ternario: (condición) ? valor_si_true : valor_si_false
            actionTexts[i].color = (i == selectedAction) ? highlightedColor : Color.black;
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        // 1. Actualizar colores
        for (int i = 0; i < moveTexts.Count; i++)
        {
            moveTexts[i].color = (i == selectedMove) ? highlightedColor : Color.black;
        }

        // 2. Actualizar detalles (PP y Tipo)
        if (move == null)
        {
            ppText.text = "PP: -/-";
            typeText.text = "Tipo: -";
        }
        else
        {
            ppText.text = $"PP: {move.Pp}/{move.Base.Pp}";
            typeText.text = move.Base.Type.ToString();
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            // Si hay movimiento asignamos nombre, si no, ponemos un guion
            moveTexts[i].text = (i < moves.Count) ? moves[i].Base.Name : "-";
        }
    }
}