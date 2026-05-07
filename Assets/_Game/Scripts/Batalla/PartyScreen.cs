using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] GameObject partyList;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    public BattleState? CalledFrom { get; set; }

    int selection = 0;
    public Pokemon SelectedMember => pokemons[selection];
    PokemonParty party;

    // --- VARIABLES CONTROL MOVIL ---
    private bool esMovil;
    private float tiempoSiguienteInput = 0f; // Cooldown para que el joystick no se mueva demasiado rapido
    private float intervaloInput = 0.2f; // Tiempo de espera entre movimientos del cursor

    public void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
         esMovil = true;
#else
        esMovil = false;
#endif
    }

    public void Init(PokemonParty pokemonParty = null)
    {
        party = pokemonParty ?? PokemonParty.GetPlayerParty();
        if (party == null)
        {
            Debug.LogError("PartyScreen: No se pudo obtener PokemonParty.");
            return;
        }

        memberSlots = partyList.GetComponentsInChildren<PartyMemberUI>();

        party.OnUpdated -= SetPartyData;
        party.OnUpdated += SetPartyData;

        SetPartyData();
    }

    public void SetPartyData()
    {
        pokemons = party.Pokemons;

        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetData(pokemons[i]);
            memberSlots[i].RefreshDisplay();
        }

        UpdateMemberSelection(selection);

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "Elige con quien combatir";
            messageText.color = Color.black;
        }
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        if (pokemons == null || pokemons.Count == 0)
        {
            return;
        }

        var prevSelection = selection;

        if (Time.time >= tiempoSiguienteInput)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() > 0.5f))
            {
                ++selection;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() < -0.5f))
            {
                --selection;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() < -0.5f))
            {
                selection += 2;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() > 0.5f))
            {
                selection -= 2;
                tiempoSiguienteInput = Time.time + intervaloInput;
            }
        }

        selection = Math.Clamp(selection, 0, pokemons.Count - 1);
        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (InputConfirmar())
        {
            onSelected?.Invoke();
        }
        else if (InputCancelar())
        {
           onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetSelected(i == selectedMember);
        }
    }

    public void SetMessageText(string message) => messageText.text = message;

    bool InputConfirmar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
        {
            return ControlesMoviles.Instance.botonInteraccion.SePresionoEsteFrame();
        }
        return Input.GetKeyDown(KeyCode.Return);
    }

    bool InputCancelar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
        {
            return ControlesMoviles.Instance.botonCorrer.SePresionoEsteFrame();
        }
        return Input.GetKeyDown(KeyCode.Escape);
    }
}