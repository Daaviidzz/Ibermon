using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] GameObject partyList;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    public void Init()
    {
        // Si partyList existe buscamos ahí, si no, en este mismo objeto (gameObject).
        // Usamos el operador ternario para decidir el origen en una línea.
        GameObject target = partyList != null ? partyList : gameObject;

        memberSlots = target.GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        if (memberSlots == null) Init(); // Inicialización de seguridad

        if (pokemons == null)
        {
            Debug.LogError("La lista de Pokemons enviada a PartyScreen es NULA.");
            return;
        }

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (memberSlots[i] == null) continue; // Si el slot es nulo, saltamos

            // Verificamos si este slot corresponde a un pokemon existente
            bool isActive = i < pokemons.Count;

            memberSlots[i].gameObject.SetActive(isActive);

            if (isActive)
                memberSlots[i].SetData(pokemons[i]);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "Elige con quien combatir";
            messageText.color = Color.black; 
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            // Pasamos 'true' si el índice coincide, 'false' si no.
            
            memberSlots[i].SetSelected(i == selectedMember);
        }
    }

    public void SetMessageText(string message) => messageText.text = message;
}