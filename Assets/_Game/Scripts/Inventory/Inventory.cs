using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    public static Inventory Instance { get; private set; }
    public List<ItemSlot> Slots => slots;
    private void Awake()
    {
        Instance = this;
    }

    public static Inventory GetInventory()
    {
        return Instance;
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
