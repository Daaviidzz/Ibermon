using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI countText;
    RectTransform rectTransform;
    private void Awake()
    {
        rectTransform=GetComponent<RectTransform>();
    }
    public TextMeshProUGUI NameText => nameText;
    public TextMeshProUGUI CountText => countText;
    public float Height
    {
        get
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            return rectTransform != null ? rectTransform.rect.height : 0f;
        }
    }

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
    }
}
