using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BattleUnit : MonoBehaviour
{
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    public bool IsPlayerUnit {  get { return isPlayerUnit; } }
    public BattleHud Hud { get { return hud; } }
    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 originalPosition;
    Color originalColor;

    
    private void Awake()
    {
        image = GetComponent<Image>();
        originalPosition = image.transform.localPosition;
        originalColor = image.color;

        
    }


    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;

        // Obtenemos el componente de imagen (SpriteRenderer)
        var image = GetComponent<UnityEngine.UI.Image>(); // Si usas UI
                                                         

        if (image != null)
        {
            // Forzamos que el color sea blanco (opaco) y no transparente
            image.color = Color.white;

            // Si es el jugador, usa el BackSprite (espalda)
            // Si NO es el jugador (es enemigo), usa el FrontSprite (frente)
            if (isPlayerUnit)
            {
                image.sprite = Pokemon.Base.BackSprite;
            }
            else
            {
                image.sprite = Pokemon.Base.FrontSprite;
            }

            hud.SetData(pokemon);
            image.color = originalColor;
            // Ejecutamos la animación de entrada
            PlayEnterAnimation();
        }

       
    }

    //Animacion de entrada de la  batalla
    public void PlayEnterAnimation()
    {
        if(isPlayerUnit)
           image.transform.localPosition = new Vector3(-500f, originalPosition.y, 0f);
        else
           image.transform.localPosition = new Vector3(500f, originalPosition.y, 0f);
       image.transform.DOLocalMoveX(originalPosition.x, 1f);

    }

    //Animacion de ataque de la  batalla
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if(isPlayerUnit)
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x + 50f, 0.25f));
          
        }
        else
        {
            sequence.Append(image.transform.DOLocalMoveX(originalPosition.x - 50f, 0.25f));
            
        }
        sequence.Append(image.transform.DOLocalMoveX(originalPosition.x, 0.25f));

    }
    //Animacion de recibir daño en la batalla
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.grey, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    //Animacion de debilitacion en la batalla
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        //Baja la imagen y la desvanece
        sequence.Append(image.transform.DOLocalMoveY(originalPosition.y -150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }


}
