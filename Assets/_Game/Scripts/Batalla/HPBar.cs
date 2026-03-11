using System.Collections;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public void SetHP(float hpNormalized)
    {
        //actualiza la barra de vida
        health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
    }
    //Anima la barra de vida para que cambie suavemente a la nueva cantidad de vida
    public IEnumerator SetHPSmooth(float newHP)
    {
        float currentHP = health.transform.localScale.x;
        float changeAmt = currentHP - newHP;

        //Define la velocidad de cambio de la barra de vida
        //Mathf.Epsilon es un valor muy pequeño para evitar problemas de precisión con los números flotantes
        while (currentHP - newHP > Mathf.Epsilon)
        {
            currentHP -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHP, 1f, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHP, 1f, 1f);
    }
}
