using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{

    [SerializeField] Color higthlighedColor;
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;


    Inventory inventory;
    int selectedItem = 0;
    List<ItemSlotUI> slotUIList;
    


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
        slotUIList = new List<ItemSlotUI>();
        // Agregar nuevos items
        foreach (var itemSlot in inventory.Slots)
        {
            var itemSlotUIObj = Instantiate(itemSlotUI, itemList.transform);
            itemSlotUIObj.SetData(itemSlot);

            slotUIList.Add(itemSlotUIObj);
        }
        UpdateItemSelection();
    }
    public void HandleUpdate(Action onBack)
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;
        selectedItem = Math.Clamp(selectedItem, 0, inventory.Slots.Count - 1);
        if (prevSelection != selectedItem)
            UpdateItemSelection();
        if (InputCancelar())
        {
            onBack?.Invoke();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < slotUIList.Count; ++i)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = higthlighedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }
        var item=inventory.Slots[selectedItem].Item;
        itemIcon.sprite = item.Icon;
        itemDescription.text= item.Description;
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
