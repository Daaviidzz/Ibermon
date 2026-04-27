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
    [SerializeField] GameObject choiceBox;

    // Listas de textos
    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;

    // Textos de detalle (PP y Tipo)
    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] TextMeshProUGUI noText;

    // --- MÉTODOS PÚBLICOS ---

    public void SetDialog(string dialog) => dialogText.text = dialog;

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";

        //Cacheamos el tiempo de espera para no crear "basura" en cada vuelta del bucle.
        var waitTime = new WaitForSeconds(1f / lettersPerSecond);

        foreach (var letter in dialog)
        {
            dialogText.text += letter;
            yield return waitTime;
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled) => dialogText.enabled = enabled;
    public void EnableActionSelector(bool enabled) => actionSelector.SetActive(enabled);

    public void EnableMoveSelector(bool enabled) 
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }
    public void EnableChoiceBox(bool enabled) => choiceBox.SetActive(enabled);

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
           
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
            ppText.text = $"PP: {move.PP}/{move.Base.PP}";
            typeText.text = move.Base.Type.ToString();
        }
        if(move.PP == 0)
        {
            ppText.color = Color.red;
        }
        else
        {
            ppText.color = Color.black;
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
    public void UpdateChoiceBox(bool yesSelected)
    {

        yesText.color = yesSelected ? highlightedColor : Color.black;
        noText.color = yesSelected ? Color.black : highlightedColor;
        
    }
}