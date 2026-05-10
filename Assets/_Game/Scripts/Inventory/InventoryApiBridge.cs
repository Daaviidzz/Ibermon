using System;
using System.Collections.Generic;
using System.Linq;
using ApiRest.Managers;
using ApiRest.Models;
using UnityEngine;

public class InventoryApiBridge
{
    private static InventoryApiBridge _instance;
    public static InventoryApiBridge Instance => _instance ??= new InventoryApiBridge();

    private static readonly Dictionary<string, ItemCategory> TipoToCategory = new()
    {
        { "curacion", ItemCategory.Items },
        { "captura", ItemCategory.Pokeballs },
        { "batalla", ItemCategory.Items },
        { "clave", ItemCategory.Items }
    };

    private static readonly Dictionary<string, string> NombreApiToRecurso = new()
    {
        { "Iberball", "Pokeball" },
        { "Poke Ball", "Pokeball" },
        { "Pokeball", "Pokeball" },
        { "Super Ball", "SuperBall" },
        { "Superball", "SuperBall" },
        { "SuperBall", "SuperBall" },
        { "Pocion", "Pocion" },
        { "Super Pocion", "SuperPocion" },
        { "SuperPocion", "SuperPocion" },
        { "Pocion Maxima", "PocionMaxima" },
        { "PocionMaxima", "PocionMaxima" }
    };

    private readonly Dictionary<string, ItemJugador> _itemsJugador = new();

    public void CargarInventario(Action onSuccess, Action<string> onError)
    {
        string partidaId = SessionManager.Instance?.PartidaId;
        if (string.IsNullOrEmpty(partidaId))
        {
            onError?.Invoke("No hay partida activa.");
            return;
        }

        if (ApiSetup.ItemJugador == null || ApiSetup.Catalogo == null)
        {
            onError?.Invoke("La API no esta preparada.");
            return;
        }

        List<ItemJugador> itemsJugador = null;
        List<ItemCatalogoResumen> catalogo = null;
        bool inventarioListo = false;
        bool catalogoListo = false;
        string error = null;

        void IntentarFinalizar()
        {
            if (!inventarioListo || !catalogoListo) return;

            if (!string.IsNullOrEmpty(error))
            {
                onError?.Invoke(error);
                return;
            }

            ConstruirInventarioLocal(itemsJugador, catalogo, onSuccess, onError);
        }

        ApiSetup.ItemJugador.ObtenerInventario(partidaId,
            items =>
            {
                itemsJugador = items;
                inventarioListo = true;
                IntentarFinalizar();
            },
            err =>
            {
                error = err;
                inventarioListo = true;
                IntentarFinalizar();
            });

        ApiSetup.Catalogo.ListarItems(
            lista =>
            {
                catalogo = lista;
                catalogoListo = true;
                IntentarFinalizar();
            },
            err =>
            {
                error = err;
                catalogoListo = true;
                IntentarFinalizar();
            });
    }

    private void ConstruirInventarioLocal(
        List<ItemJugador> itemsJugador,
        List<ItemCatalogoResumen> catalogo,
        Action onSuccess,
        Action<string> onError)
    {
        Inventory inventory = Inventory.GetInventory();
        if (inventory == null)
        {
            onError?.Invoke("No se encontro Inventory en la escena.");
            return;
        }

        itemsJugador ??= new List<ItemJugador>();
        catalogo ??= new List<ItemCatalogoResumen>();

        inventory.ClearAllSlots();
        _itemsJugador.Clear();

        ItemBase[] itemsLocales = Resources.LoadAll<ItemBase>("Items");

        foreach (ItemJugador itemJugador in itemsJugador)
        {
            ItemCatalogoResumen itemCatalogo =
                catalogo.FirstOrDefault(item => item.numero == itemJugador.item_catalogo_id);

            if (itemCatalogo == null)
            {
                Debug.LogWarning($"[InventoryApiBridge] Item catalogo {itemJugador.item_catalogo_id} no encontrado.");
                continue;
            }

            string nombreRecurso = ObtenerNombreRecurso(itemCatalogo.nombre);
            ItemBase itemLocal = itemsLocales.FirstOrDefault(item =>
                item != null && (item.name == nombreRecurso || item.Name == nombreRecurso));

            if (itemLocal == null)
            {
                Debug.LogWarning($"[InventoryApiBridge] Item '{nombreRecurso}' no existe en Resources/Items.");
                continue;
            }

            ItemCategory categoria = ObtenerCategoria(itemCatalogo.tipo);
            inventory.AddItem(itemLocal, itemJugador.cantidad, categoria);
            _itemsJugador[itemLocal.Name] = itemJugador;
        }

        onSuccess?.Invoke();
    }

    public void RegistrarUso(string nombreRecursoLocal, int cantidadRestante, Action onFalloRecargar)
    {
        if (string.IsNullOrEmpty(nombreRecursoLocal))
            return;

        if (!_itemsJugador.TryGetValue(nombreRecursoLocal, out ItemJugador itemJugador))
        {
            Debug.LogWarning($"[InventoryApiBridge] Item '{nombreRecursoLocal}' no esta en datos API.");
            return;
        }

        string partidaId = SessionManager.Instance?.PartidaId;
        if (string.IsNullOrEmpty(partidaId) || ApiSetup.ItemJugador == null)
        {
            onFalloRecargar?.Invoke();
            return;
        }

        if (cantidadRestante <= 0)
        {
            ApiSetup.ItemJugador.EliminarItem(partidaId, itemJugador.id,
                () => _itemsJugador.Remove(nombreRecursoLocal),
                err =>
                {
                    Debug.LogWarning($"[InventoryApiBridge] Error eliminando {nombreRecursoLocal}: {err}");
                    onFalloRecargar?.Invoke();
                });
            return;
        }

        ApiSetup.ItemJugador.ActualizarItem(partidaId, itemJugador.id, cantidadRestante,
            itemActualizado => _itemsJugador[nombreRecursoLocal] = itemActualizado,
            err =>
            {
                Debug.LogWarning($"[InventoryApiBridge] Error actualizando {nombreRecursoLocal}: {err}");
                onFalloRecargar?.Invoke();
            });
    }

    private static string ObtenerNombreRecurso(string nombreApi)
    {
        if (string.IsNullOrWhiteSpace(nombreApi))
            return string.Empty;

        return NombreApiToRecurso.TryGetValue(nombreApi, out string nombreRecurso)
            ? nombreRecurso
            : nombreApi.Replace(" ", string.Empty);
    }

    private static ItemCategory ObtenerCategoria(string tipoApi)
    {
        if (string.IsNullOrWhiteSpace(tipoApi))
            return ItemCategory.Items;

        return TipoToCategory.TryGetValue(tipoApi.ToLowerInvariant(), out ItemCategory categoria)
            ? categoria
            : ItemCategory.Items;
    }
}
