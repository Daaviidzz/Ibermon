using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory { Items, Pokeballs }
public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;

    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;
    public static Inventory Instance { get; private set; }
    
    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS","POKEBALLS"
    };
    

    private void Awake()
    {
        Instance = this;
        allSlots = new List<List<ItemSlot>>()
        {
            slots, pokeballSlots
        };
    }

    public static Inventory GetInventory()
    {
        return Instance;
    }
    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
       return allSlots[categoryIndex];
    }

    public void ClearAllSlots()
    {
        slots.Clear();
        pokeballSlots.Clear();
        OnUpdated?.Invoke();
    }

    public void AddItem(ItemBase item, int count, ItemCategory category)
    {
        var targetList = category == ItemCategory.Pokeballs ? pokeballSlots : slots;
        var existingSlot = targetList.FirstOrDefault(s => s.Item == item);

        if (existingSlot != null)
            existingSlot.Count += count;
        else
            targetList.Add(new ItemSlot(item, count));

        OnUpdated?.Invoke();
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon,int selectedCategory)
    {
        var currentSlots = GetSlotsByCategory(selectedCategory);

        var item = currentSlots[itemIndex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item, selectedCategory);
            return item;
        }
        return null;
    }
    public void RemoveItem(ItemBase item, int category)
    {
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(s => s.Item == item);
        if (itemSlot == null) return;

        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            currentSlots.Remove(itemSlot);
        }
        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot() { }

    public ItemSlot(ItemBase item, int count)
    {
        this.item = item;
        this.count = count;
    }

    public ItemBase Item => item;
    public int Count 
    {
        get => count;
        set => count = value;
    }
}
