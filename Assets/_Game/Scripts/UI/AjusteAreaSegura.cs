using UnityEngine;

// Ajusta el RectTransform al Screen.safeArea para evitar notch / barra de gestos
[RequireComponent(typeof(RectTransform))]
public class AjusteAreaSegura : MonoBehaviour
{
    private RectTransform rt;
    private Rect ultimaAreaSegura;
    private Vector2Int ultimaResolucion;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        Aplicar();
    }

    private void Update()
    {
        if (Screen.safeArea != ultimaAreaSegura
            || Screen.width != ultimaResolucion.x
            || Screen.height != ultimaResolucion.y)
        {
            Aplicar();
        }
    }

    private void Aplicar()
    {
        var safe = Screen.safeArea;
        var min = safe.position;
        var max = safe.position + safe.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        ultimaAreaSegura = safe;
        ultimaResolucion = new Vector2Int(Screen.width, Screen.height);
    }
}
