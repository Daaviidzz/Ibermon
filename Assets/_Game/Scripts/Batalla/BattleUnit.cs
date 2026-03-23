using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections; // Importante: Librería para animaciones suaves (DOTween).

public class BattleUnit : MonoBehaviour
{

    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit => isPlayerUnit;
    public BattleHud Hud => hud;

    // Propiedad auto-implementada para guardar la referencia del Pokémon actual.
    public Pokemon Pokemon { get; set; }

    Image image; // Referencia al componente visual del Pokémon.
    Vector3 originalPosition; // Guardamos dónde estaba originalmente para volver tras animaciones.
    Color originalColor;      // Guardamos el color original para restaurarlo tras recibir daño.

    private void Awake()
    {
        // Obtenemos las referencias una sola vez al inicio para optimizar rendimiento.
        image = GetComponent<Image>();
        originalPosition = image.transform.localPosition;
        originalColor = image.color;
    }

    // Configura la unidad con los datos del Pokémon al iniciar batalla o cambiar criatura.
    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;

        // Verificamos que tengamos la imagen asignada (por seguridad).
        if (image != null)
        {
            // Restauramos el color original por si el anterior murió o recibió daño.
            image.color = originalColor;

            // Asignamos el Sprite correcto dependiendo de si es Jugador (Espalda) o Enemigo (Frente).
            image.sprite = isPlayerUnit ? Pokemon.Base.BackSprite : Pokemon.Base.FrontSprite;

            hud.gameObject.SetActive(true);
            // Actualizamos la interfaz (Nombre, Nivel, Vida) en el HUD.
            hud.SetData(pokemon);

            transform.localScale = new Vector3(1, 1, 1);
            // Iniciamos la animación de entrada deslizante.
            PlayEnterAnimation();
        }
    }


    // Animación de entrada: El Pokémon se desliza desde fuera de la pantalla hacia su posición.
    public void PlayEnterAnimation()
    {
        // Si es jugador entra desde la izquierda (-500), si es enemigo desde la derecha (500).
        float startX = isPlayerUnit ? -500f : 500f;

        // Colocamos la imagen fuera de pantalla instantáneamente.
        image.transform.localPosition = new Vector3(startX, originalPosition.y, 0f);

        // Movemos suavemente hacia la posición original en X durante 1 segundo.
        image.transform.DOLocalMoveX(originalPosition.x, 1f);
    }

    // Animación de ataque: Un pequeño empujón hacia adelante y vuelta atrás.
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();

        // Determina la dirección del empujón: +50 (derecha) si es jugador, -50 (izquierda) si es enemigo.
        float moveDir = isPlayerUnit ? 50f : -50f;

        // 1. Empuje rápido hacia el oponente (0.25 segundos).
        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + moveDir, 0.25f));

        // 2. Vuelta a la posición original (0.25 segundos).
        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.25f));
    }

    // Animación de recibir daño: Parpadeo en color gris.
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();

        // 1. Cambia el color a gris instantáneamente 
        sequence.Append(image.DOColor(Color.gray, 0.1f));

        // 2. Vuelve al color original del sprite.
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    // Animación de debilitarse (Muerte): Baja hacia el suelo y se desvanece.
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();

        // Append: Mueve la unidad hacia abajo (eje Y) 150 unidades.
        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y - 150f, 0.5f));

        // Join: AL MISMO TIEMPO, reduce la opacidad (alpha) a 0 (invisible).
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0f, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPosition.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1f, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPosition.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }
}