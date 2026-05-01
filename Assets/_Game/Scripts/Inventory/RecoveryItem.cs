using UnityEngine;

[CreateAssetMenu(menuName ="Items/Crear nuevo recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;
    [Header("PP")]
    [SerializeField] bool restoreMaxPP;
    [SerializeField] int ppAmount;
    [Header("Estados")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;
    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon pokemon)
    {
        if(hpAmount > 0)
        {
            if(pokemon.HP == pokemon.MaxHp)
            {
                return false;
            }
            pokemon.IncreaseHP(hpAmount);
        }
        return true;
    }
}
