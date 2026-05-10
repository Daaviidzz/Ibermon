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
    Action<ItemBase> OnItemUsed;

    // --- VARIABLES CONTROL MOVIL ---
    private bool esMovil;
    private float tiempoSiguienteInput = 0f; // Cooldown para que el joystick no se mueva demasiado rápido
    private float intervaloInput = 0.2f; // Tiempo de espera entre movimientos del cursor

    public void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
    esMovil = true;
#else
        esMovil = false;
#endif
        inventory = Inventory.GetInventory();

        if (inventory == null)
        {
            Debug.LogError("INVENTORY ES NULL - No existe un objeto Inventory en la escena");
            return;
        }

        itemListRect = itemList.GetComponent<RectTransform>();
        AplicarEstiloMochila();
    }
    private void Start()
    {
        if (!AsegurarInventory()) return;
        inventory.OnUpdated += UpdateItemList;
        UpdateItemList();
    }

    // intenta recuperar el Inventory si se perdio (cambio de escena, etc.)
    private bool AsegurarInventory()
    {
        if (inventory != null) return true;
        inventory = Inventory.GetInventory();
        return inventory != null;
    }

    public void Abrir()
    {
        state = InventoryUIState.ItemSelection;
        selectedItem = 0;
        ActualizarTextoCategoria();
        CargarInventarioDesdeAPI();
    }

    private void CargarInventarioDesdeAPI()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("[InventoryUI] Inventory no disponible.");
            return;
        }

        InventoryApiBridge.Instance.CargarInventario(
            onSuccess: () => UpdateItemList(),
            onError: error =>
            {
                Debug.LogError($"[InventoryUI] {error}");
                UpdateItemList();
                StartCoroutine(MostrarErrorMochila());
            });
    }

    private IEnumerator MostrarErrorMochila()
    {
        yield return new WaitForSeconds(0.3f);
        if (DialogManager.Instance != null)
            yield return DialogManager.Instance.ShowDialogText("No se pudo cargar la mochila desde el servidor.");
    }
    void UpdateItemList()
    {
        if (!AsegurarInventory()) return;

        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();

        var slotsCategoria = inventory.GetSlotsByCategory(selectedCategory);
        foreach (var itemSlot in slotsCategoria)
        {
            var itemSlotUIObj = Instantiate(itemSlotUI, itemList.transform);
            itemSlotUIObj.SetData(itemSlot);
            AplicarEstiloSlot(itemSlotUIObj);
            slotUIList.Add(itemSlotUIObj);
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, Mathf.Max(0, slotsCategoria.Count - 1));

        // forzar rebuild del layout
        if (itemListRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemListRect);
        }

        if (slotUIList.Count > 0)
            UpdateItemSelection();
        else
            ResetSelection(true);
    }
    public void HandleUpdate(Action onBack,Action<ItemBase> onItemUsed=null)
    {
        if (!AsegurarInventory()) return;

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

            var slotsCategoria = inventory.GetSlotsByCategory(selectedCategory);
            selectedItem = Mathf.Clamp(selectedItem, 0, Mathf.Max(0, slotsCategoria.Count - 1));
            

            if(prevCategory != selectedCategory)
            {
                ResetSelection(false);
                ActualizarTextoCategoria();
                UpdateItemList();
            }else if (prevSelection != prevCategory)
            {
               UpdateItemSelection();
            }

            if (prevSelection != selectedItem)
                UpdateItemSelection();

            if (InputConfirmar())
            {
                if (slotUIList != null && slotUIList.Count > 0)
                {
                    ItemSelected();
                }
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
        if (!AsegurarInventory()) yield break;

        state = InventoryUIState.Busy;

        // Desuscribirse para evitar que RemoveItem destruya los slots durante la corrutina
        inventory.OnUpdated -= UpdateItemList;

        var slotsActuales = inventory.GetSlotsByCategory(selectedCategory);
        int cantidadAntes = 0;
        if (selectedItem < slotsActuales.Count)
            cantidadAntes = slotsActuales[selectedItem].Count;

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if (usedItem != null)
        {
            if(!(usedItem is PokeballItem))
                yield return DialogManager.Instance.ShowDialogText($"Usaste {usedItem.Name}");

            int cantidadRestante = cantidadAntes - 1;
            InventoryApiBridge.Instance?.RegistrarUso(
                usedItem.Name,
                cantidadRestante,
                onFalloRecargar: () => CargarInventarioDesdeAPI());

            OnItemUsed?.Invoke(usedItem);
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
        if (!AsegurarInventory()) return;

        var slots = inventory.GetSlotsByCategory(selectedCategory);
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; ++i)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = higthlighedColor;
                if (!slotUIList[i].NameText.text.StartsWith("> "))
                    slotUIList[i].NameText.text = "> " + slotUIList[i].NameText.text.TrimStart('>', ' ');
            }
            else
            {
                slotUIList[i].NameText.color = Color.black;
                slotUIList[i].NameText.text = slotUIList[i].NameText.text.TrimStart('>', ' ');
            }
        }

        if(slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            if (itemIcon != null)
            {
                itemIcon.enabled = item.Icon != null;
                itemIcon.sprite = item.Icon;
            }

            if (itemDescription != null)
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
    void ResetSelection(bool mostrarVacia)
    {
        selectedItem = 0;
        
        if (upArrow != null)
            upArrow.gameObject.SetActive(false);
        if (downArrow != null)
            downArrow.gameObject.SetActive(false);

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (itemDescription != null)
        {
            itemDescription.text = mostrarVacia
                ? "No hay objetos guardados en esta categoria."
                : "";
        }

        // El mensaje va en la descripcion para no pisar las flechas.
    }

    void MostrarMensajeVacio()
    {
        if (itemList == null) return;

        var mensajeObj = new GameObject("EmptyInventoryMessage", typeof(RectTransform), typeof(Image));
        mensajeObj.transform.SetParent(itemList.transform, false);

        var rect = mensajeObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -60f);
        rect.sizeDelta = new Vector2(540f, 105f);

        var fondo = mensajeObj.GetComponent<Image>();
        fondo.color = new Color(0.98f, 0.98f, 0.95f, 0.96f);

        var textoObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textoObj.transform.SetParent(mensajeObj.transform, false);

        var textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(18f, 10f);
        textoRect.offsetMax = new Vector2(-18f, -10f);

        var texto = textoObj.GetComponent<TextMeshProUGUI>();
        texto.text = "Mochila vacia\nNo llevas objetos en este bolsillo.";
        texto.fontSize = 23f;
        texto.color = new Color(0.12f, 0.12f, 0.12f);
        texto.alignment = TextAlignmentOptions.Center;
    }

    void ActualizarTextoCategoria()
    {
        if (categoryText != null && selectedCategory >= 0 && selectedCategory < Inventory.ItemCategories.Count)
            categoryText.text = Inventory.ItemCategories[selectedCategory];
    }

    void AplicarEstiloMochila()
    {
        var rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
        {
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
        }

        var fondo = GetComponent<Image>();
        if (fondo != null)
            fondo.color = new Color(0.98f, 0.97f, 0.92f, 1f);

        var panelLista = itemList != null ? itemList.GetComponentInParent<Image>() : null;
        if (panelLista != null)
        {
            panelLista.enabled = true;
            panelLista.color = new Color(1f, 1f, 1f, 0.92f);
        }

        if (categoryText != null)
        {
            var rect = categoryText.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, 0.76f);
            rect.anchorMax = new Vector2(0.42f, 0.92f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            categoryText.fontSize = 30f;
            categoryText.color = new Color(0.05f, 0.05f, 0.05f);
            categoryText.alignment = TextAlignmentOptions.Center;
            categoryText.margin = new Vector4(72f, 0f, 72f, 0f);

            AjustarFlechasCategoria();
        }

        if (itemIcon != null)
        {
            var rect = itemIcon.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.16f, 0.46f);
            rect.anchorMax = new Vector2(0.32f, 0.70f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            itemIcon.preserveAspect = true;
        }

        if (itemList != null)
        {
            var scrollRect = itemList.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                var rect = scrollRect.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.42f, 0.46f);
                rect.anchorMax = new Vector2(0.88f, 0.88f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        if (itemDescription != null)
        {
            var rect = itemDescription.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, 0.06f);
            rect.anchorMax = new Vector2(0.94f, 0.24f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            itemDescription.fontSize = 22f;
            itemDescription.color = new Color(0.05f, 0.05f, 0.05f);
            itemDescription.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    void AjustarFlechasCategoria()
    {
        foreach (Transform hijo in categoryText.transform)
        {
            var rect = hijo.GetComponent<RectTransform>();
            if (rect == null) continue;

            if (hijo.name.Contains("Left"))
            {
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(28f, 0f);
            }
            else if (hijo.name.Contains("Rigth") || hijo.name.Contains("Right"))
            {
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-28f, 0f);
            }

            rect.sizeDelta = new Vector2(56f, 56f);
        }
    }

    void AplicarEstiloSlot(ItemSlotUI slot)
    {
        if (slot == null) return;

        if (slot.NameText != null)
        {
            slot.NameText.fontSize = 28f;
            slot.NameText.color = Color.black;
            slot.NameText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        if (slot.CountText != null)
        {
            slot.CountText.fontSize = 23f;
            slot.CountText.color = Color.black;
            slot.CountText.alignment = TextAlignmentOptions.MidlineRight;
        }
    }
    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData(); // Reinicializar PartyScreen para asegurar datos frescos
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
    void ItemSelected()
    {
        if(selectedCategory==(int) ItemCategory.Pokeballs)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
    }
}
