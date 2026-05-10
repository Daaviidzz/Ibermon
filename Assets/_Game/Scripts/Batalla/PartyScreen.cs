using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private bool esMovil;
    private float tiempoSiguienteInput = 0f;
    private float intervaloInput = 0.2f;

    public void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
         esMovil = true;
#else
        esMovil = false;
#endif
    }

    private void OnDestroy()
    {
        if (party != null)
            party.OnUpdated -= SetPartyData;
    }

    public void Init(PokemonParty pokemonParty = null)
    {
        party = pokemonParty ?? PokemonParty.GetPlayerParty();
        if (party == null)
        {
            Debug.LogError("PartyScreen: No se pudo obtener PokemonParty.");
            return;
        }

        if (partyList == null)
        {
            Debug.LogError("PartyScreen: partyList no asignado.");
            return;
        }

        memberSlots = partyList.GetComponentsInChildren<PartyMemberUI>(true);

        if (memberSlots == null || memberSlots.Length == 0)
        {
            Debug.LogError("PartyScreen: No se encontraron PartyMemberUI en partyList.");
            return;
        }

        party.OnUpdated -= SetPartyData;
        party.OnUpdated += SetPartyData;

        // Usar GridLayoutGroup del prefab en vez de posicionar a mano
        var grid = partyList.GetComponent<GridLayoutGroup>();
        if (grid != null) grid.enabled = true;

        if (CatalogoCache.Instance != null && !CatalogoCache.Instance.EstaListo)
            Debug.LogWarning("[PartyScreen] CatalogoCache no listo al iniciar.");

        SetPartyData();
    }

    public void SetPartyData()
    {
        if (party == null) return;
        pokemons = party.Pokemons;
        pokemons ??= new List<Pokemon>();

        if ((memberSlots == null || memberSlots.Length == 0) && partyList != null)
            memberSlots = partyList.GetComponentsInChildren<PartyMemberUI>(true);

        if (memberSlots == null) return;

        bool slotsInvalidos = false;
        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (memberSlots[i] == null)
            {
                slotsInvalidos = true;
                break;
            }
        }

        if (slotsInvalidos)
        {
            memberSlots = partyList != null ? partyList.GetComponentsInChildren<PartyMemberUI>(true) : null;
            if (memberSlots == null || memberSlots.Length == 0) return;
        }

        selection = Mathf.Clamp(selection, 0, Mathf.Max(0, pokemons.Count - 1));

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (memberSlots[i] == null) continue;

            bool tienePokemon = i < pokemons.Count;
            memberSlots[i].gameObject.SetActive(tienePokemon);

            if (tienePokemon)
                memberSlots[i].SetData(pokemons[i]);
        }

        if (pokemons.Count > 0)
            UpdateMemberSelection(selection);

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = pokemons.Count > 0
                ? "Elige un Ibermon"
                : "No hay Ibermon en el equipo.";
            messageText.color = Color.black;
        }
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        if (pokemons == null || pokemons.Count == 0) return;

        var prevSelection = selection;

        if (Time.time >= tiempoSiguienteInput)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() > 0.5f))
            { ++selection; tiempoSiguienteInput = Time.time + intervaloInput; }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (esMovil && ControlesMoviles.Instance.joystick.Horizontal() < -0.5f))
            { --selection; tiempoSiguienteInput = Time.time + intervaloInput; }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() < -0.5f))
            { selection += 2; tiempoSiguienteInput = Time.time + intervaloInput; }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || (esMovil && ControlesMoviles.Instance.joystick.Vertical() > 0.5f))
            { selection -= 2; tiempoSiguienteInput = Time.time + intervaloInput; }
        }

        selection = Math.Clamp(selection, 0, pokemons.Count - 1);
        if (selection != prevSelection)
            UpdateMemberSelection(selection);

        if (InputConfirmar())
            onSelected?.Invoke();
        else if (InputCancelar())
            onBack?.Invoke();
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        if (pokemons == null || memberSlots == null) return;
        int total = Mathf.Min(pokemons.Count, memberSlots.Length);
        for (int i = 0; i < total; i++)
            memberSlots[i].SetSelected(i == selectedMember);
    }

    public void SetMessageText(string message) => messageText.text = message;

    bool InputConfirmar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
            return ControlesMoviles.Instance.botonInteraccion.SePresionoEsteFrame();
        return Input.GetKeyDown(KeyCode.Return);
    }

    bool InputCancelar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
            return ControlesMoviles.Instance.botonCorrer.SePresionoEsteFrame();
        return Input.GetKeyDown(KeyCode.Escape);
    }
}
