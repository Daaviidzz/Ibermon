using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI messageText;

    [SerializeField] GameObject partyList;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    public void Init()
    {
        // Buscamos los slots específicamente dentro de partyList
        if (partyList != null)
        {
            memberSlots = partyList.GetComponentsInChildren<PartyMemberUI>(true);
        }
        else
        {
            //  busca en todo lo que cuelgue de PartyScreen
            memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        }

    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        // PROTECCIÓN 1: Si Init no se ejecutó, forzarlo
        if (memberSlots == null) Init();

        // PROTECCIÓN 2: Si la lista de pokemons llega vacía, salir para no crashear
        if (pokemons == null)
        {
            Debug.LogError("La lista de Pokemons enviada a PartyScreen es NULA.");
            return;
        }

        for (int i = 0; i < memberSlots.Length; i++)
        {
            // PROTECCIÓN 3: Si un slot es nulo (por error en Unity), saltarlo
            if (memberSlots[i] == null) continue;

            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        if (messageText != null) 
        { messageText.gameObject.SetActive(true);
            messageText.text = "Elige con quien combatir";
            // FORZAMOS COLOR NEGRO Y ALFA AL MÁXIMO
            messageText.color = new Color(0, 0, 0, 1); // R=0, G=0, B=0 (Negro), A=1 (Opaco)
           
        }
            
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
                memberSlots[i].SetSelected(false);
        }
    }
    public void SetMessageText(string message)
    {
        messageText.text=message;
    }
}
