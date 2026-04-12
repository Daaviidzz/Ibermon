using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Arregla el bug de Unity 6 que lanza ArgumentException al arrastrar un GameObject
/// desde la Jerarquía hasta el Project Browser para convertirlo en prefab.
///
/// Causa del bug: Unity 6 usa EntityId (UInt64) internamente, pero el sistema de
/// drag-and-drop del Project Browser sigue esperando Int32. Al soltar, intenta
/// convertir UInt64 a Int32 y peta.
///
/// Solución: este script intercepta el DragPerform ANTES de que llegue al código
/// buggeado de Unity y crea el prefab directamente con PrefabUtility.
///
/// Instalación: este fichero debe estar en cualquier carpeta llamada "Editor" dentro
/// de Assets/. La carpeta _Game/Editor/ ya es válida.
/// No hace falta hacer nada más — Unity lo carga automáticamente al abrir el editor.
/// </summary>
[InitializeOnLoad]
public static class PrefabDragFix
{
    static PrefabDragFix()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect itemRect)
    {
        Event evt = Event.current;

        // Solo nos interesa cuando el ratón está encima de este item del Project
        if (!itemRect.Contains(evt.mousePosition))
            return;

        // Solo cuando se está arrastrando algo
        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            return;

        // Filtrar: solo GameObjects que viven en una escena abierta (no assets del Project)
        GameObject[] objetosArrastrados = DragAndDrop.objectReferences
            .OfType<GameObject>()
            .Where(go => go.scene.IsValid())
            .ToArray();

        if (objetosArrastrados.Length == 0)
            return;

        // Indicar a Unity que aceptamos este drag (cambia el cursor a "copiar")
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            // Determinar la carpeta de destino a partir del GUID del item bajo el cursor
            string rutaDestino = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(rutaDestino))
                rutaDestino = System.IO.Path.GetDirectoryName(rutaDestino);

            foreach (GameObject go in objetosArrastrados)
            {
                string rutaPrefab = AssetDatabase.GenerateUniqueAssetPath(
                    $"{rutaDestino}/{go.name}.prefab");

                // Crea el prefab y reconecta la instancia de la escena al nuevo asset
                PrefabUtility.SaveAsPrefabAssetAndConnect(
                    go, rutaPrefab, InteractionMode.UserAction);

                Debug.Log($"[PrefabDragFix] Prefab creado en: {rutaPrefab}");
            }

            // Consumir el evento para que Unity no intente procesarlo (evita el crash)
            evt.Use();
        }
    }
}
