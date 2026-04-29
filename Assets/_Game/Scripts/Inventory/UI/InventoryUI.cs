using System;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    Inventory inventory;


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
        inventory = Inventory.GetInventory();
    }
    private void Start()
    {
        UpdateItemList();
    }
    void UpdateItemList()
    {
        // Limpiar lista actual
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }
        // Agregar nuevos items
        foreach (var itemSlot in inventory.Slots)
        {
            var itemSlotUIObj = Instantiate(itemSlotUI, itemList.transform);
            itemSlotUIObj.SetData(itemSlot);
        }
    }
    public void HandleUpdate(Action onBack)
    {
        if (InputCancelar())
        {
            onBack?.Invoke();
        }
    }
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
