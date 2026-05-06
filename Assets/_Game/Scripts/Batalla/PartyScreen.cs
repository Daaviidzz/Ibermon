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
    /// <summary>
    /// Party Screen puede ser llamado desde diferentes estados de batalla (por ejemplo, selección de ataque o selección de pokemon).
    /// </summary>
    public BattleState? CalledFrom { get; set; }

    int selection = 0;
    public Pokemon SelectedMember 
    { 
        get 
        {
            if (pokemons == null || pokemons.Count == 0)
            {
                Debug.LogError("? SelectedMember: No hay Pokémons disponibles");
                return null;
            }
            return pokemons[selection];
        }
    }
    PokemonParty party;
    private bool initialized = false; // Flag para asegurar que Init() solo se ejecuta una vez

    // --- VARIABLES CONTROL MOVIL ---
    private bool esMovil;
    private float tiempoSiguienteInput = 0f; // Cooldown para que el joystick no se mueva demasiado rápido
    private float intervaloInput = 0.2f; // Tiempo de espera entre movimientos del cursor

    public void Awake()
    {
        // Detectar si estamos en móvil o PC
#if UNITY_ANDROID || UNITY_IOS
         esMovil = true;
#else
        esMovil = false;
#endif
    }
    public void Init(PokemonParty pokemonParty = null)
    {
        GameObject target = partyList != null ? partyList : gameObject;
        memberSlots = target.GetComponentsInChildren<PartyMemberUI>(true);

        party = pokemonParty ?? PokemonParty.GetPlayerParty();

        // Suscribirse solo si no estaba ya suscrito
        party.OnUpdated -= SetPartyData;
        party.OnUpdated += SetPartyData;

        SetPartyData();
    }


    public void SetPartyData()
    {
        // Auto-recuperación: si party o memberSlots son null, los obtenemos aquí
        if (party == null)
            party = PokemonParty.GetPlayerParty();

        if (memberSlots == null)
        {
            GameObject target = partyList != null ? partyList : gameObject;
            memberSlots = target.GetComponentsInChildren<PartyMemberUI>(true);
        }

        if (party == null || memberSlots == null)
        {
            Debug.LogError("PartyScreen: No se pudo obtener party o memberSlots.");
            return;
        }

        pokemons = party.Pokemons;

        if (pokemons == null || pokemons.Count == 0)
        {
           
            Debug.LogWarning("PartyScreen: Lista vacía, el equipo aún no se ha cargado.");
            return;
        }

        for (int i = 0; i < memberSlots.Length; i++)
        {
            if (memberSlots[i] == null) continue;
            bool isActive = i < pokemons.Count;
            memberSlots[i].gameObject.SetActive(isActive);
            if (isActive)
            {
                memberSlots[i].SetData(pokemons[i]);
                memberSlots[i].RefreshDisplay();
            }
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
            return; // simplemente salir sin hacer nada
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
        if(selection != prevSelection)
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
            // Pasamos 'true' si el índice coincide, 'false' si no.
            
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

    // Detectar "Escape" o Botón Correr (que usaremos como botón B/Atrás)
    bool InputCancelar()
    {
        if (esMovil && ControlesMoviles.Instance != null)
        {
            // Usamos el botón de correr como "Atrás" en los menús
            return ControlesMoviles.Instance.botonCorrer.SePresionoEsteFrame();
        }
        return Input.GetKeyDown(KeyCode.Escape);
    }
}