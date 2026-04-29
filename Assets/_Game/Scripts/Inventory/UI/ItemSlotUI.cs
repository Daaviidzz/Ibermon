using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI countText;

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"X {itemSlot.Count}";
    }
}
