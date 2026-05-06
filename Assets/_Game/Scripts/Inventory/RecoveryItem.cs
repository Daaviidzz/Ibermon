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
        
        if(revive || maxRevive)
        {
            if(pokemon.HP > 0)
            {
                return false;
            }
            if(maxRevive)
            {
                pokemon.IncreaseHP(pokemon.MaxHp);
            }
            else if(revive)
                pokemon.IncreaseHP(pokemon.MaxHp/2);

            pokemon.CureStatus();

            return true;
        }
        if(pokemon.HP== 0)
        {
            return false;
        }

        if(hpAmount > 0|| restoreMaxHP )
        {
            if(pokemon.HP == pokemon.MaxHp)
            {
                return false;
            }
            if (restoreMaxHP)
            {
               pokemon.IncreaseHP(pokemon.MaxHp);
                
            }
            else
                pokemon.IncreaseHP(hpAmount);
        }

        if (recoverAllStatus || status != ConditionID.none)
        {
            if (pokemon.Status == null && pokemon.VolatileStatus == null)
            {
                return false;
            }
            if(recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if (pokemon.Status.Id == status )
                    pokemon.CureStatus();
                else if(pokemon.VolatileStatus.Id==status)
                    pokemon.CureVolatileStatus();
                else
                    return false;
            }
                
        }

        if(restoreMaxPP)
        {
           pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if(ppAmount > 0)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
