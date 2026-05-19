using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using UnityEngine;

// Se encarga de dar 2 items aleatorios al jugador cuando gana un combate.
// La eleccion se hace con un sistema de pesos: items mas comunes tienen mas peso.
// La comparacion de nombres es tolerante: ignora mayusculas, tildes y espacios.
public static class RecompensaCombate
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // TABLA DE RAREZAS — modifica los pesos para ajustar las probabilidades
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static readonly Dictionary<string, int> pesosPokeballs = new()
    {
        { "Pokeball",     70 },  // Comun       → ~66%
        { "SuperBall",    25 },  // Poco comun  → ~24%
        { "UltraBall",     9 },  // Rara        → ~8.5%
        { "MasterBall",    1 }   // Legendaria  → ~1%  (pon 0 si NO quieres que aparezca)
    };

    private static readonly Dictionary<string, int> pesosCurativos = new()
    {
        { "Pocion",        50 },  // Comun (cura HP basico)
        { "Super Pocion",  25 },  // Poco comun
        { "Pocion Maxima",  8 },  // Rara (cura HP al maximo)
        { "Ether",         10 },  // Poco comun (recupera PP)
        { "Revivir",        5 },  // Rara (revive Ibermon debilitado)
        { "Antidoto",       2 }   // Muy rara (cura veneno)
    };

    private const int pesoPorDefecto = 5;

    // Cache para no spamear el warning del mismo item varias veces
    private static readonly HashSet<string> _itemsYaAvisados = new();

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    // Anade 2 items aleatorios al inventario del jugador.
    // dialogBox: caja de dialogo del combate para mostrar mensajes.
    // onDone: callback que se llama cuando termina (incluida la sincronizacion API).
    public static IEnumerator DarRecompensa(BattleDialogBox dialogBox, System.Action onDone)
    {
        ItemBase[] todos = Resources.LoadAll<ItemBase>("Items");
        List<ItemBase> curativos = todos.OfType<RecoveryItem>().Cast<ItemBase>().ToList();
        List<ItemBase> pokeballs = todos.OfType<PokeballItem>().Cast<ItemBase>().ToList();

        if (curativos.Count == 0 && pokeballs.Count == 0)
        {
            Debug.LogWarning("[RecompensaCombate] No hay items en Resources/Items");
            onDone?.Invoke();
            yield break;
        }

        ItemBase recompensaCurativa = ElegirPorPeso(curativos, pesosCurativos);
        ItemBase recompensaPokeball = ElegirPorPeso(pokeballs, pesosPokeballs);

        if (recompensaCurativa != null)
        {
            bool listo = false;
            InventoryApiBridge.Instance.AnadirItemConSync(recompensaCurativa, 1, () => listo = true);
            yield return dialogBox.TypeDialog($"¡Has obtenido 1 {recompensaCurativa.Name}!");
            yield return new WaitUntil(() => listo);
            yield return new WaitForSeconds(0.5f);
        }

        if (recompensaPokeball != null)
        {
            bool listo = false;
            InventoryApiBridge.Instance.AnadirItemConSync(recompensaPokeball, 1, () => listo = true);
            yield return dialogBox.TypeDialog($"¡Has obtenido 1 {recompensaPokeball.Name}!");
            yield return new WaitUntil(() => listo);
            yield return new WaitForSeconds(0.5f);
        }

        onDone?.Invoke();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // ALGORITMO DE LA RULETA PONDERADA
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private static ItemBase ElegirPorPeso(List<ItemBase> items, Dictionary<string, int> tablaPesos)
    {
        if (items == null || items.Count == 0) return null;

        int pesoTotal = 0;
        foreach (ItemBase item in items)
            pesoTotal += ObtenerPeso(item, tablaPesos);

        if (pesoTotal <= 0) return items[Random.Range(0, items.Count)];

        int tirada = Random.Range(0, pesoTotal);
        int acumulado = 0;
        foreach (ItemBase item in items)
        {
            acumulado += ObtenerPeso(item, tablaPesos);
            if (tirada < acumulado) return item;
        }
        return items[items.Count - 1];
    }

    private static int ObtenerPeso(ItemBase item, Dictionary<string, int> tablaPesos)
    {
        string nombreNorm = Normalizar(item.Name);
        foreach (var kvp in tablaPesos)
        {
            if (Normalizar(kvp.Key) == nombreNorm)
                return kvp.Value;
        }

        if (_itemsYaAvisados.Add(item.Name))
        {
            Debug.LogWarning($"[RecompensaCombate] Item '{item.Name}' sin peso definido, " +
                             $"usando peso por defecto={pesoPorDefecto}.");
        }
        return pesoPorDefecto;
    }

    // Convierte un nombre a forma "canonica": minusculas, sin tildes, sin espacios.
    // "Poción Máxima" → "pocionmaxima"
    private static string Normalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        string sinTildes = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in sinTildes)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().ToLowerInvariant().Replace(" ", "");
    }
}