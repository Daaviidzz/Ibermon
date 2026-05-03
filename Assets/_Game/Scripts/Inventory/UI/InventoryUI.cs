using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection,PartySelection,Busy}
public class InventoryUI : MonoBehaviour
{

    [SerializeField] Color higthlighedColor;
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;

    InventoryUIState state;
    RectTransform itemListRect;
    Inventory inventory;
    int selectedItem = 0;
    int selectedCategory = 0;

    List<ItemSlotUI> slotUIList;
    const int ITEMS_IN_VIEWPORT = 8; // Número de items visibles sin necesidad de hacer scroll
    Action OnItemUsed;

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
        itemListRect = itemList.GetComponent<RectTransform>();
    }
    private void Start()
    {
        inventory.OnUpdated -= UpdateItemList; // evitar doble suscripción
        inventory.OnUpdated += UpdateItemList;
        UpdateItemList();
    }
    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var itemSlotUIObj = Instantiate(itemSlotUI, itemList.transform);
            itemSlotUIObj.SetData(itemSlot);
            slotUIList.Add(itemSlotUIObj);
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, Mathf.Max(0, inventory.GetSlotsByCategory(selectedCategory).Count - 1));

        // forzar rebuild del layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemListRect);

        if (slotUIList.Count > 0)
            UpdateItemSelection();
    }
    public void HandleUpdate(Action onBack,Action onItemUsed=null)
    {
        this.OnItemUsed = onItemUsed;
        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                --selectedItem;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                ++selectedCategory;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --selectedCategory;

            if(selectedCategory > Inventory.ItemCategories.Count - 1)
            {
                selectedCategory = 0;
            }
            else if(selectedCategory < 0)
            {
                selectedCategory = Inventory.ItemCategories.Count - 1;
            }

            selectedItem = Math.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);
            

            if(prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }else if (prevSelection != prevCategory)
            {
               UpdateItemSelection();
            }

            if (prevSelection != selectedItem)
                UpdateItemSelection();

            if (InputConfirmar())
            {
                OpenPartyScreen();
            }
            else if (InputCancelar())
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };
            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }
    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        // Desuscribirse para evitar que RemoveItem destruya los slots durante la corrutina
        inventory.OnUpdated -= UpdateItemList;

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if (usedItem != null)
        {
            yield return DialogManager.Instance.ShowDialogText($"Usaste {usedItem.Name}");
            OnItemUsed?.Invoke();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"No se pudo usar el item");
        }

        // Ahora que la corrutina terminó, volver a suscribirse y actualizar
        inventory.OnUpdated += UpdateItemList;
        UpdateItemList();
        yield return null;
        ClosePartyScreen();
    }
    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; ++i)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = higthlighedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }

        if(slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();

    }
    void HandleScrolling()
    {
        if (slotUIList == null || slotUIList.Count == 0) return;
        if (slotUIList[0] == null) return;

        if (slotUIList.Count <= ITEMS_IN_VIEWPORT) return;

        float itemHeight = slotUIList[0].Height;
        float scrollPos = Mathf.Clamp(selectedItem - ITEMS_IN_VIEWPORT / 2, 0, selectedItem) * itemHeight;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > ITEMS_IN_VIEWPORT / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + ITEMS_IN_VIEWPORT / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
    void ResetSelection()
    {
        selectedItem = 0;
        
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
        partyScreen.Init(); // Reinicializar PartyScreen para asegurar datos frescos
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
        
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
    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnUpdated -= UpdateItemList;
    }
}
