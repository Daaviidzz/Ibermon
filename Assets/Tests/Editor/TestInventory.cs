using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// =====================================================================================
//  PR-09 — Tests para Inventory (Bloque 2 - Combate)
//
//  Inventory es el singleton que gestiona los objetos del jugador. Mantiene dos
//  listas separadas: ITEMS (curativos, etc.) y POKEBALLS, accesibles por categoría.
//
//  Aquí testeamos las operaciones básicas:
//   · AddItem (suma cantidad si existe, crea slot si no)
//   · RemoveItem (decrementa y elimina al llegar a 0)
//   · GetSlotsByCategory devuelve la lista correcta
//   · ItemSlot mantiene la cuenta correctamente
// =====================================================================================
public class TestInventory
{
    private Inventory _inv;
    private GameObject _go;

    [SetUp]
    public void Setup()
    {
        _go = new GameObject("InventoryTest");
        _inv = _go.AddComponent<Inventory>();

        // Los campos privados [SerializeField] no se inicializan automáticamente
        // sin el Inspector — los seteamos a listas vacías por reflection.
        var fSlots = typeof(Inventory).GetField("slots",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var fPokeballs = typeof(Inventory).GetField("pokeballSlots",
            BindingFlags.NonPublic | BindingFlags.Instance);
        fSlots.SetValue(_inv, new List<ItemSlot>());
        fPokeballs.SetValue(_inv, new List<ItemSlot>());

        // Volvemos a llamar Awake (Unity ya lo llamó pero con campos null)
        var mAwake = typeof(Inventory).GetMethod("Awake",
            BindingFlags.NonPublic | BindingFlags.Instance);
        mAwake?.Invoke(_inv, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.DestroyImmediate(_go);
    }

    // -----------------------------------------------------------------------------------
    //  Helper: crea un ItemBase mock genérico
    // -----------------------------------------------------------------------------------
    private static ItemBase CrearItemBase(string nombre)
    {
        var item = ScriptableObject.CreateInstance<RecoveryItem>();
        TestHelpers.SetField(item, "name", nombre);
        return item;
    }

    // -----------------------------------------------------------------------------------
    //  PR-09.1 - AddItem crea un slot nuevo si no existe
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR09_01_AddItem_CreaSlotNuevo()
    {
        var pocion = CrearItemBase("Pocion");

        _inv.AddItem(pocion, count: 3, ItemCategory.Items);

        var slots = _inv.GetSlotsByCategory((int)ItemCategory.Items);
        Assert.AreEqual(1, slots.Count, "Debe haber 1 slot en la categoría ITEMS");
        Assert.AreEqual(3, slots[0].Count, "Cantidad debe ser 3");

        Object.DestroyImmediate(pocion);
    }

    // -----------------------------------------------------------------------------------
    //  PR-09.2 - AddItem suma cantidad si el item ya existe
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR09_02_AddItem_SumaSiExiste()
    {
        var pocion = CrearItemBase("Pocion");

        _inv.AddItem(pocion, count: 3, ItemCategory.Items);
        _inv.AddItem(pocion, count: 2, ItemCategory.Items);

        var slots = _inv.GetSlotsByCategory((int)ItemCategory.Items);
        Assert.AreEqual(1, slots.Count, "Solo debe haber 1 slot (mismo item)");
        Assert.AreEqual(5, slots[0].Count, "La cantidad total debe ser 3+2 = 5");

        Object.DestroyImmediate(pocion);
    }

    // -----------------------------------------------------------------------------------
    //  PR-09.3 - AddItem en categorías distintas crea slots separados
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR09_03_AddItem_CategoriasSeparadas()
    {
        var pocion = CrearItemBase("Pocion");
        var pokeball = CrearItemBase("Iberball");

        _inv.AddItem(pocion, 1, ItemCategory.Items);
        _inv.AddItem(pokeball, 5, ItemCategory.Pokeballs);

        var items = _inv.GetSlotsByCategory((int)ItemCategory.Items);
        var pokeballs = _inv.GetSlotsByCategory((int)ItemCategory.Pokeballs);

        Assert.AreEqual(1, items.Count, "1 slot en ITEMS");
        Assert.AreEqual(1, pokeballs.Count, "1 slot en POKEBALLS");
        Assert.AreEqual(5, pokeballs[0].Count);

        Object.DestroyImmediate(pocion);
        Object.DestroyImmediate(pokeball);
    }

    // -----------------------------------------------------------------------------------
    //  PR-09.4 - RemoveItem decrementa contador y elimina slot al llegar a 0
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR09_04_RemoveItem_DecrementaYEliminaA0()
    {
        var pocion = CrearItemBase("Pocion");
        _inv.AddItem(pocion, 2, ItemCategory.Items);

        _inv.RemoveItem(pocion, (int)ItemCategory.Items);
        var slots = _inv.GetSlotsByCategory((int)ItemCategory.Items);
        Assert.AreEqual(1, slots.Count, "Todavía hay slot");
        Assert.AreEqual(1, slots[0].Count, "Cantidad ahora es 1");

        _inv.RemoveItem(pocion, (int)ItemCategory.Items);
        slots = _inv.GetSlotsByCategory((int)ItemCategory.Items);
        Assert.AreEqual(0, slots.Count, "Tras llegar a 0, el slot se elimina");

        Object.DestroyImmediate(pocion);
    }

    // -----------------------------------------------------------------------------------
    //  PR-09.5 - ClearAllSlots vacía las dos categorías
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR09_05_ClearAllSlots_VaciaTodasLasCategorias()
    {
        var pocion = CrearItemBase("Pocion");
        var ball = CrearItemBase("Iberball");
        _inv.AddItem(pocion, 3, ItemCategory.Items);
        _inv.AddItem(ball, 5, ItemCategory.Pokeballs);

        _inv.ClearAllSlots();

        Assert.AreEqual(0, _inv.GetSlotsByCategory((int)ItemCategory.Items).Count);
        Assert.AreEqual(0, _inv.GetSlotsByCategory((int)ItemCategory.Pokeballs).Count);

        Object.DestroyImmediate(pocion);
        Object.DestroyImmediate(ball);
    }

    // -----------------------------------------------------------------------------------
    //  PR-09.6 - OnUpdated event se dispara tras Add y Remove
    // -----------------------------------------------------------------------------------
    [Test]
    public void PR09_06_OnUpdated_SeDispara()
    {
        var pocion = CrearItemBase("Pocion");
        int veces = 0;
        _inv.OnUpdated += () => veces++;

        _inv.AddItem(pocion, 1, ItemCategory.Items);
        _inv.RemoveItem(pocion, (int)ItemCategory.Items);

        Assert.AreEqual(2, veces, "OnUpdated debe haberse disparado 2 veces (Add + Remove)");

        Object.DestroyImmediate(pocion);
    }
}
