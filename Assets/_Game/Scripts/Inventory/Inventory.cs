using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    
    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon)
    {
        var item = slots[itemIndex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item);
            return item;
        }
        return null;
    }
    public void RemoveItem(ItemBase item)
    {
       var itemSlot = slots.FirstOrDefault(s => s.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            slots.Remove(itemSlot);
        }
        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count 
    {
        get => count;
        set => count = value;
    }
}
