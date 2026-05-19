using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
        { "PocionMaxima", "PocionMaxima" },
        { "Antidoto", "Antidoto" },
        { "Ether", "Ether" },
        { "Revivir", "Revivir" }
    };

    private readonly Dictionary<string, ItemJugador> _itemsJugador = new();

    public void CargarInventario(Action onSuccess, Action<string> onError)
    {
        Inventory inventory = Inventory.GetInventory();
        if (inventory != null)
            inventory.ClearAllSlots();
        _itemsJugador.Clear();

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
                Debug.Log($"[InventoryApiBridge] Inventario API recibido: {itemsJugador?.Count ?? 0} items.");
                inventarioListo = true;
                IntentarFinalizar();
            },
            err =>
            {
                Debug.LogWarning($"[InventoryApiBridge] Error cargando inventario API: {err}");
                error = err;
                inventarioListo = true;
                IntentarFinalizar();
            });

        ApiSetup.Catalogo.ListarItems(
            lista =>
            {
                catalogo = lista;
                Debug.Log($"[InventoryApiBridge] Catalogo items API recibido: {catalogo?.Count ?? 0} items.");
                catalogoListo = true;
                IntentarFinalizar();
            },
            err =>
            {
                Debug.LogWarning($"[InventoryApiBridge] Error cargando catalogo items API: {err}");
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
            string claveRecurso = NormalizarClave(nombreRecurso);
            ItemBase itemLocal = itemsLocales.FirstOrDefault(item =>
                item != null &&
                (NormalizarClave(item.name) == claveRecurso ||
                 NormalizarClave(item.Name) == claveRecurso));

            if (itemLocal == null)
            {
                Debug.LogWarning($"[InventoryApiBridge] Item '{nombreRecurso}' no existe en Resources/Items.");
                continue;
            }

            ItemCategory categoria = ObtenerCategoria(itemCatalogo.tipo);
            inventory.AddItem(itemLocal, itemJugador.cantidad, categoria);
            RegistrarItemJugador(itemLocal, nombreRecurso, itemJugador);
        }

        Debug.Log($"[InventoryApiBridge] Inventario local construido desde API: {itemsJugador.Count} registros.");
        onSuccess?.Invoke();
    }

    public void RegistrarUso(string nombreRecursoLocal, int cantidadRestante, Action onFalloRecargar)
    {
        if (string.IsNullOrEmpty(nombreRecursoLocal))
            return;

        string claveNormalizada = NormalizarClave(nombreRecursoLocal);
        if (!_itemsJugador.TryGetValue(nombreRecursoLocal, out ItemJugador itemJugador) &&
            !_itemsJugador.TryGetValue(claveNormalizada, out itemJugador))
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
                () => QuitarItemJugador(itemJugador),
                err =>
                {
                    Debug.LogWarning($"[InventoryApiBridge] Error eliminando {nombreRecursoLocal}: {err}");
                    onFalloRecargar?.Invoke();
                });
            return;
        }

        ApiSetup.ItemJugador.ActualizarItem(partidaId, itemJugador.id, cantidadRestante,
            ActualizarItemJugador,
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

        if (NombreApiToRecurso.TryGetValue(nombreApi, out string nombreRecurso))
            return nombreRecurso;

        string claveApi = NormalizarClave(nombreApi);
        foreach (var nombre in NombreApiToRecurso)
        {
            if (NormalizarClave(nombre.Key) == claveApi)
                return nombre.Value;
        }

        return nombreApi.Replace(" ", string.Empty);
    }

    private void RegistrarItemJugador(ItemBase itemLocal, string nombreRecurso, ItemJugador itemJugador)
    {
        GuardarClave(itemLocal.Name, itemJugador);
        GuardarClave(itemLocal.name, itemJugador);
        GuardarClave(nombreRecurso, itemJugador);
    }

    private void GuardarClave(string clave, ItemJugador itemJugador)
    {
        if (string.IsNullOrWhiteSpace(clave)) return;

        _itemsJugador[clave] = itemJugador;
        _itemsJugador[NormalizarClave(clave)] = itemJugador;
    }

    private void QuitarItemJugador(ItemJugador itemJugador)
    {
        if (itemJugador == null) return;

        var claves = _itemsJugador
            .Where(par => par.Value != null && par.Value.id == itemJugador.id)
            .Select(par => par.Key)
            .ToList();

        foreach (string clave in claves)
            _itemsJugador.Remove(clave);
    }

    private void ActualizarItemJugador(ItemJugador itemActualizado)
    {
        if (itemActualizado == null) return;

        var claves = _itemsJugador
            .Where(par => par.Value != null && par.Value.id == itemActualizado.id)
            .Select(par => par.Key)
            .ToList();

        foreach (string clave in claves)
            _itemsJugador[clave] = itemActualizado;
    }

    private static string NormalizarClave(string clave)
    {
        if (string.IsNullOrWhiteSpace(clave))
            return string.Empty;

        string normalizada = clave.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (char caracter in normalizada)
        {
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(caracter);
            if (categoria == UnicodeCategory.NonSpacingMark || char.IsWhiteSpace(caracter))
                continue;

            builder.Append(char.ToLowerInvariant(caracter));
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static ItemCategory ObtenerCategoria(string tipoApi)
    {
        if (string.IsNullOrWhiteSpace(tipoApi))
            return ItemCategory.Items;

        return TipoToCategory.TryGetValue(tipoApi.ToLowerInvariant(), out ItemCategory categoria)
            ? categoria
            : ItemCategory.Items;
    }
    // Anade un item al inventario LOCAL y lo persiste en el servidor.
    // Si el item ya existe en la partida actualiza la cantidad; si no, lo crea.
    // Anade un item al inventario LOCAL y lo persiste en el servidor.
    // Si el item ya existe en la partida actualiza la cantidad; si no, lo crea.
    public void AnadirItemConSync(ItemBase item, int cantidad, System.Action onDone)
    {
        if (item == null || cantidad <= 0) { onDone?.Invoke(); return; }

        Inventory inv = Inventory.GetInventory();
        if (inv == null) { onDone?.Invoke(); return; }

        // 1. Anadir al inventario local (se ve en la UI inmediatamente)
        ItemCategory categoria = item is PokeballItem ? ItemCategory.Pokeballs : ItemCategory.Items;
        inv.AddItem(item, cantidad, categoria);

        // 2. Sincronizar con la API
        string partidaId = SessionManager.Instance?.PartidaId;
        if (string.IsNullOrEmpty(partidaId) || ApiSetup.ItemJugador == null || ApiSetup.Catalogo == null)
        {
            onDone?.Invoke();
            return;
        }

        // Si ya teniamos este item en la partida, actualizamos cantidad
        if (_itemsJugador.TryGetValue(item.Name, out ItemJugador existente))
        {
            int nuevaCantidad = existente.cantidad + cantidad;
            ApiSetup.ItemJugador.ActualizarItem(partidaId, existente.id, nuevaCantidad,
                actualizado => { existente.cantidad = actualizado.cantidad; onDone?.Invoke(); },
                err =>
                {
                    Debug.LogWarning($"[InventoryApiBridge] Error actualizando item: {err}");
                    onDone?.Invoke();
                });
            return;
        }

        // Si NO existe, buscamos el numero del catalogo y lo creamos
        ApiSetup.Catalogo.ListarItems(
            catalogo =>
            {
                // Comparacion tolerante: ignora mayusculas, tildes y espacios
                string nombreItemNorm = NormalizarTexto(item.Name);
                ItemCatalogoResumen cat = catalogo.FirstOrDefault(c =>
                    NormalizarTexto(c.nombre) == nombreItemNorm ||
                    NormalizarTexto(ObtenerNombreRecurso(c.nombre)) == nombreItemNorm);

                if (cat == null)
                {
                    Debug.LogWarning($"[InventoryApiBridge] Item '{item.Name}' no esta en el catalogo de la API. " +
                                     $"Nombres disponibles: {string.Join(", ", catalogo.Select(c => $"'{c.nombre}'"))}");
                    onDone?.Invoke();
                    return;
                }

                ApiSetup.ItemJugador.AnadirItem(partidaId, cat.numero, cantidad,
                    nuevoItem =>
                    {
                        _itemsJugador[item.Name] = nuevoItem;
                        onDone?.Invoke();
                    },
                    err =>
                    {
                        Debug.LogWarning($"[InventoryApiBridge] Error anadiendo item: {err}");
                        onDone?.Invoke();
                    });
            },
            err => { onDone?.Invoke(); });
    }

    // Normaliza un texto: minusculas, sin tildes, sin espacios. Sirve para
    // comparar nombres entre el inventario local y el catalogo de la API.
    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";

        string sinTildes = texto.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (char c in sinTildes)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant().Replace(" ", "");
    }
}
