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
    public Pokemon SelectedMember => pokemons[selection];

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