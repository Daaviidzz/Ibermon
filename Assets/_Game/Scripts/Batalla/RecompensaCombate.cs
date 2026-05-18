using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Se encarga de dar 2 items aleatorios al jugador cuando gana un combate:
// uno de tipo curativo (RecoveryItem) y una pokeball (PokeballItem).
//
// La eleccion se hace con un SISTEMA DE PESOS: a cada item se le asigna un
// peso numerico segun su rareza. La probabilidad de que salga cada item es
// peso / suma_total_de_pesos. Asi una Pokeball normal (peso 70) sale unas
// 70 veces mas que una MasterBall (peso 1).
public static class RecompensaCombate
{
    // TABLA DE RAREZAS — modificar aqui para ajustar las probabilidades
    //
    // Los nombres DEBEN coincidir con el campo "Name" del ItemBase en el
    // Inspector de Unity (no con el nombre del archivo .asset).

    private static readonly Dictionary<string, int> pesosPokeballs = new()
    {
        { "Pokeball",    70 },  // Comun       → ~66% de probabilidad
        { "SuperBall",   25 },  // Poco comun  → ~24%
        { "UltraBall",    9 },  // Rara        → ~8.5%
        { "MasterBall",   1 }   // Legendaria  → ~1% 
    };

    private static readonly Dictionary<string, int> pesosCurativos = new()
    {
        { "Pocion",       60 },  // Comun
        { "SuperPocion",  25 },  // Poco comun
        { "PocionMaxima",  8 },  // Rara (cura HP al maximo)
        { "Revivir",       5 },  // Rara (revive a un Ibermon debilitado)
        { "Antidoto",      2 }   // Muy rara
    };

    // Peso por defecto que se aplica a items cuyo nombre NO este en las tablas
    // de arriba. Si añades un item nuevo y se te olvida ponerlo aqui, saldra
    // con esta frecuencia (relativamente raro).
    private const int pesoPorDefecto = 5;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    // Anade 2 items aleatorios al inventario del jugador.
    // dialogBox: caja de dialogo del combate para mostrar mensajes.
    // onDone: callback que se llama cuando todo ha terminado.
    public static IEnumerator DarRecompensa(BattleDialogBox dialogBox, System.Action onDone)
    {
        // 1. Cargar todos los items disponibles desde Resources/Items
        ItemBase[] todos = Resources.LoadAll<ItemBase>("Items");
        List<ItemBase> curativos = todos.OfType<RecoveryItem>().Cast<ItemBase>().ToList();
        List<ItemBase> pokeballs = todos.OfType<PokeballItem>().Cast<ItemBase>().ToList();

        if (curativos.Count == 0 && pokeballs.Count == 0)
        {
            Debug.LogWarning("[RecompensaCombate] No hay items en Resources/Items");
            onDone?.Invoke();
            yield break;
        }

        // 2. Elegir uno aleatorio de cada categoria, ponderado por rareza
        ItemBase recompensaCurativa = ElegirPorPeso(curativos, pesosCurativos);
        ItemBase recompensaPokeball = ElegirPorPeso(pokeballs, pesosPokeballs);

        // 3. Anadir cada item al inventario (local + API) y mostrar mensaje
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
    // SISTEMA DE PESOS (algoritmo de la "ruleta ponderada")
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    // Selecciona un item aleatorio de la lista usando la tabla de pesos.
    // Algoritmo: imagina una ruleta donde cada item ocupa un trozo proporcional
    // a su peso. Tiramos un numero al azar y vemos en que trozo cae.
    private static ItemBase ElegirPorPeso(List<ItemBase> items, Dictionary<string, int> tablaPesos)
    {
        if (items == null || items.Count == 0) return null;

        // Suma de todos los pesos (el "tamaño total de la ruleta")
        int pesoTotal = 0;
        foreach (ItemBase item in items)
            pesoTotal += ObtenerPeso(item, tablaPesos);

        // Si por alguna razon todos los pesos son 0, devolvemos uno al azar plano
        if (pesoTotal <= 0) return items[Random.Range(0, items.Count)];

        // Tiramos un valor entre 0 y pesoTotal-1, y avanzamos por la lista
        // sumando pesos hasta que el acumulado supere la tirada
        int tirada = Random.Range(0, pesoTotal);
        int acumulado = 0;
        foreach (ItemBase item in items)
        {
            acumulado += ObtenerPeso(item, tablaPesos);
            if (tirada < acumulado) return item;
        }

        // Defensivo: en teoria nunca llegamos aqui
        return items[items.Count - 1];
    }

    private static int ObtenerPeso(ItemBase item, Dictionary<string, int> tablaPesos)
    {
        if (tablaPesos.TryGetValue(item.Name, out int peso))
            return peso;

        Debug.LogWarning($"[RecompensaCombate] Item '{item.Name}' no esta en la tabla de pesos. " +
                         $"Usando pesoPorDefecto={pesoPorDefecto}. Añadelo a las tablas si quieres ajustar su rareza.");
        return pesoPorDefecto;
    }
}