using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] Color highligthedColor;
    int currentSelection = 0;
   public void SetMoveData(List<MoveBase> currentMoves,MoveBase newMove)
   {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;

        }
        moveTexts[currentMoves.Count].text = newMove.Name;
    }
    public void HandleMoveSelection(bool esMovil,Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() < -0.5f))
        {
            ++currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() > 0.5f))
        {
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, PokemonBase.MaxNumOfMoves);
        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            onSelected?.Invoke(currentSelection);
        }
    }
    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < PokemonBase.MaxNumOfMoves+1; i++)
        {
            if (i == selection)
                moveTexts[i].color = highligthedColor;
            else
                moveTexts[i].color = Color.black;
        }
    }
}
