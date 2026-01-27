using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

   
    private void setHP(float hpNormalized)
    {
        //actualiza la barra de vida
        health.transform.localScale = new Vector3(hpNormalized, 1f, 1f);
    }
}
