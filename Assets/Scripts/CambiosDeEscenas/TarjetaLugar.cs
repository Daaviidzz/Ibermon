using System.Collections;
using UnityEngine;
using TMPro;

public class TarjetaLugar : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelTarjeta;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Configuración")]
    [SerializeField] private float duracionFadeIn = 0.5f;
    [SerializeField] private float tiempoVisible = 2.5f;
    [SerializeField] private float duracionFadeOut = 0.5f;

    private TextMeshProUGUI textoNombreLugar;

    private void Awake()
    {
        // Cogemos el TextMeshPro que ya está en el panel
        textoNombreLugar = panelTarjeta.GetComponentInChildren<TextMeshProUGUI>();

        // Asegurarse de que empieza oculto
        if (panelTarjeta != null)
        {
            panelTarjeta.SetActive(false);
        }
    }

    private void Start()
    {
        // Mostrar la tarjeta al entrar en la escena
        Invoke("MostrarTarjeta", 0.2f);
    }

    private void MostrarTarjeta()
    {
        // El texto ya está puesto en el TextMeshPro desde el Inspector
        // Solo tenemos que animar el panel
        StopAllCoroutines();
        StartCoroutine(AnimarTarjeta());
    }

    private IEnumerator AnimarTarjeta()
    {
        // Activar el panel y empezar invisible
        panelTarjeta.SetActive(true);
        canvasGroup.alpha = 0f;

        // Fade In
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionFadeIn)
        {
            tiempoTranscurrido += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, tiempoTranscurrido / duracionFadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Esperar visible
        yield return new WaitForSeconds(tiempoVisible);

        // Fade Out
        tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionFadeOut)
        {
            tiempoTranscurrido += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / duracionFadeOut);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Desactivar el panel
        panelTarjeta.SetActive(false);
    }
}