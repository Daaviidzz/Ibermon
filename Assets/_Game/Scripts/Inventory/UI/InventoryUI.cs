using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] bool aplicarEstiloPorCodigo = false;

    InventoryUIState state;
    RectTransform itemListRect;
    Inventory inventory;
    int selectedItem = 0;
    int selectedCategory = 0;

    List<ItemSlotUI> slotUIList;
    const int ITEMS_IN_VIEWPORT = 8; // Número de items visibles sin necesidad de hacer scroll
    Action<ItemBase> OnItemUsed;
    bool cargandoInventario;

    // --- VARIABLES CONTROL MOVIL ---
    private bool esMovil;

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

        AsegurarReferenciasUI();

        if (itemList == null)
        {
            Debug.LogError("[InventoryUI] No se encontro itemList/Content en la mochila.");
            return;
        }

        itemListRect = itemList.GetComponent<RectTransform>();
        if (aplicarEstiloPorCodigo)
            AplicarEstiloMochila();
    }
    private void Start()
    {
        if (!AsegurarInventory()) return;
        inventory.OnUpdated += UpdateItemList;
        UpdateItemList();
    }

    private void OnEnable()
    {
        if (!AsegurarInventory()) return;

        AplicarEstiloMochila();

        if (state == InventoryUIState.ItemSelection)
            CargarInventarioDesdeAPI();
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
        Debug.Log("[InventoryUI] Abriendo mochila y cargando inventario desde API.");
        state = InventoryUIState.ItemSelection;
        selectedItem = 0;
        ActualizarTextoCategoria();
        CargarInventarioDesdeAPI();
    }

    private void CargarInventarioDesdeAPI()
    {
        if (cargandoInventario) return;
        cargandoInventario = true;

        if (Inventory.Instance == null)
        {
            Debug.LogError("[InventoryUI] Inventory no disponible.");
            cargandoInventario = false;
            return;
        }

        InventoryApiBridge.Instance.CargarInventario(
            onSuccess: () =>
            {
                cargandoInventario = false;
                UpdateItemList();
            },
            onError: error =>
            {
                cargandoInventario = false;
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
        if (itemList == null || itemSlotUI == null)
        {
            Debug.LogError("[InventoryUI] Faltan referencias de lista o slot.");
            return;
        }

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

        if (itemListRect == null) return;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > ITEMS_IN_VIEWPORT / 2;
        if (upArrow != null)
            upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = selectedItem + ITEMS_IN_VIEWPORT / 2 < slotUIList.Count;
        if (downArrow != null)
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
            rootRect.localScale = Vector3.one;
            rootRect.localRotation = Quaternion.identity;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
        }

        var fondo = GetComponent<Image>();
        if (fondo != null)
            fondo.color = new Color(0.98f, 0.97f, 0.92f, 1f);

        var barraSuperior = BuscarHijo(transform, "BarraSuperior") as RectTransform;
        if (barraSuperior != null)
        {
            barraSuperior.localScale = Vector3.one;
            barraSuperior.localRotation = Quaternion.identity;
            barraSuperior.anchorMin = new Vector2(0.06f, 0.76f);
            barraSuperior.anchorMax = new Vector2(0.46f, 0.92f);
            barraSuperior.pivot = new Vector2(0.5f, 0.5f);
            barraSuperior.offsetMin = Vector2.zero;
            barraSuperior.offsetMax = Vector2.zero;
        }

        var panelDescripcion = BuscarHijo(transform, "DescripcionItem") as RectTransform;
        if (panelDescripcion != null)
        {
            panelDescripcion.localScale = Vector3.one;
            panelDescripcion.localRotation = Quaternion.identity;
            panelDescripcion.anchorMin = Vector2.zero;
            panelDescripcion.anchorMax = Vector2.one;
            panelDescripcion.pivot = new Vector2(0.5f, 0.5f);
            panelDescripcion.offsetMin = Vector2.zero;
            panelDescripcion.offsetMax = Vector2.zero;
        }

        var barraLateral = BuscarHijo(transform, "BarraLateralDerecha") as RectTransform;
        if (barraLateral != null)
        {
            barraLateral.localScale = Vector3.one;
            barraLateral.localRotation = Quaternion.identity;
            barraLateral.anchorMin = new Vector2(0.90f, 0.46f);
            barraLateral.anchorMax = new Vector2(0.96f, 0.88f);
            barraLateral.pivot = new Vector2(0.5f, 0.5f);
            barraLateral.offsetMin = Vector2.zero;
            barraLateral.offsetMax = Vector2.zero;
        }

        var panelLista = itemList != null ? itemList.GetComponentInParent<Image>() : null;
        if (panelLista != null)
        {
            panelLista.enabled = true;
            panelLista.color = new Color(1f, 1f, 1f, 0.92f);
        }

        if (categoryText != null)
        {
            var rect = categoryText.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            categoryText.fontSize = 26f;
            categoryText.color = new Color(0.05f, 0.05f, 0.05f);
            categoryText.alignment = TextAlignmentOptions.Center;
            categoryText.margin = new Vector4(42f, 0f, 42f, 0f);
            categoryText.textWrappingMode = TextWrappingModes.NoWrap;
            categoryText.overflowMode = TextOverflowModes.Ellipsis;
            categoryText.enableAutoSizing = true;
            categoryText.fontSizeMin = 14f;
            categoryText.fontSizeMax = 26f;

            AjustarFlechasCategoria();
        }

        if (itemIcon != null)
        {
            var rect = itemIcon.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
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
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
                rect.anchorMin = new Vector2(0.42f, 0.46f);
                rect.anchorMax = new Vector2(0.88f, 0.88f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            var grid = itemList.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.cellSize = new Vector2(420f, 44f);
                grid.spacing = new Vector2(0f, 4f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 1;
            }
        }

        if (itemDescription != null)
        {
            var rect = itemDescription.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
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
        var barraSuperior = BuscarHijo(transform, "BarraSuperior");
        if (barraSuperior == null) return;

        int flechaIndex = 0;
        foreach (Transform hijo in barraSuperior)
        {
            var rect = hijo.GetComponent<RectTransform>();
            if (rect == null) continue;
            if (hijo == categoryText.transform) continue;

            if (hijo.name.Contains("Flecha"))
            {
                bool izquierda = flechaIndex == 0;
                rect.localScale = Vector3.one;
                rect.localRotation = izquierda
                    ? Quaternion.Euler(0f, 0f, 90f)
                    : Quaternion.Euler(0f, 0f, -90f);
                rect.anchorMin = izquierda ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
                rect.anchorMax = izquierda ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = izquierda ? new Vector2(26f, 0f) : new Vector2(-26f, 0f);
                rect.sizeDelta = new Vector2(38f, 38f);

                flechaIndex++;
            }
        }
    }

    void AplicarEstiloSlot(ItemSlotUI slot)
    {
        if (slot == null) return;

        var layout = slot.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
            layout.enabled = false;

        var slotRect = slot.GetComponent<RectTransform>();
        if (slotRect != null)
        {
            slotRect.localScale = Vector3.one;
            slotRect.localRotation = Quaternion.identity;
            slotRect.anchorMin = new Vector2(0f, 1f);
            slotRect.anchorMax = new Vector2(1f, 1f);
            slotRect.pivot = new Vector2(0.5f, 1f);
        }

        if (slot.NameText != null)
        {
            var rect = slot.NameText.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(-88f, 0f);

            slot.NameText.fontSize = 26f;
            slot.NameText.color = Color.black;
            slot.NameText.alignment = TextAlignmentOptions.MidlineLeft;
            slot.NameText.textWrappingMode = TextWrappingModes.NoWrap;
            slot.NameText.overflowMode = TextOverflowModes.Ellipsis;
        }

        if (slot.CountText != null)
        {
            var rect = slot.CountText.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(76f, 0f);

            slot.CountText.fontSize = 23f;
            slot.CountText.color = Color.black;
            slot.CountText.alignment = TextAlignmentOptions.MidlineRight;
            slot.CountText.textWrappingMode = TextWrappingModes.NoWrap;
            slot.CountText.overflowMode = TextOverflowModes.Overflow;
        }
    }

    void AsegurarReferenciasUI()
    {
        if (itemList == null)
            itemList = BuscarHijo(transform, "Content")?.gameObject;

        if (categoryText == null)
            categoryText = BuscarHijo(transform, "NombreCategoria")?.GetComponent<TextMeshProUGUI>();

        if (itemIcon == null)
            itemIcon = BuscarHijo(transform, "Itemicon")?.GetComponent<Image>();

        if (itemDescription == null)
            itemDescription = BuscarHijo(transform, "Description")?.GetComponent<TextMeshProUGUI>();

        Transform barraLateral = BuscarHijo(transform, "BarraLateralDerecha");
        if (barraLateral != null)
        {
            if (upArrow == null)
                upArrow = BuscarHijo(barraLateral, "FlechaArriba")?.GetComponent<Image>();

            if (downArrow == null)
                downArrow = BuscarHijo(barraLateral, "FlechaAbajo")?.GetComponent<Image>();
        }

        if (partyScreen == null)
            partyScreen = FindAnyObjectByType<PartyScreen>(FindObjectsInactive.Include);
    }

    Transform BuscarHijo(Transform raiz, string nombre)
    {
        if (raiz == null) return null;

        foreach (Transform hijo in raiz)
        {
            if (hijo.name == nombre)
                return hijo;

            Transform encontrado = BuscarHijo(hijo, nombre);
            if (encontrado != null)
                return encontrado;
        }

        return null;
    }

    void OpenPartyScreen()
    {
        if (partyScreen == null)
        {
            Debug.LogError("[InventoryUI] No hay PartyScreen asignado.");
            return;
        }

        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData(); // Reinicializar PartyScreen para asegurar datos frescos
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        if (partyScreen != null)
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
            if (SceneManager.GetActiveScene().name != "Combate")
            {
                StartCoroutine(MostrarPokeballFueraDeCombate());
                return;
            }

            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartyScreen();
        }
    }

    IEnumerator MostrarPokeballFueraDeCombate()
    {
        yield return DialogManager.Instance.ShowDialogText("Solo puedes usar Pokeballs en combate.");
    }
}
