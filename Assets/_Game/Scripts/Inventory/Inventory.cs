using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;

    public List<ItemSlot> Slots => slots;
    public static Inventory GetInventory() 
    {
        var inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError(" ERROR: Inventory no encontrado en la escena. " +
                "Asegúrate de que hay un GameObject con el componente Inventory adjunto.");
        }
        else
        {
            Debug.Log($"✓ Inventory encontrado: {inventory.gameObject.name}");  
        }
        return inventory;
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count => count;
}
