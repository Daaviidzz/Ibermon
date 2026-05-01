using System.Collections.Generic;
using UnityEngine;

//Esto luego seria una base de datos, pero por ahora lo dejo asi para probar el sistema de estados
public class ConditionsDB 
{
   public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }

    }
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>() 
    { 
        { ConditionID.psn, new Condition() 
            { 
                Name = "Veneno", 
                StartMessage = "¡ha sido envenenado!",
                OnAfterTurn=(Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} sufre daño por el veneno.");
                }
            } 
        },
        { ConditionID.brn, new Condition() 
            { 
                Name = "Quemadura", 
                StartMessage = "¡ha sido quemado!" ,
                OnAfterTurn=(Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp/16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} sufre daño por la quemadura.");
                }
            } 
        },
        { ConditionID.slp, new Condition() 
            { 
                Name = "Sueño", 
                StartMessage = "¡ha caído en sueño!" ,
                OnStart=(Pokemon pokemon)=>
                {
                    pokemon.StatusTime = Random.Range(1,4);
                },
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    if(pokemon.StatusTime<=0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} se ha despertado.");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está dormido y no puede moverse.");
                    return false;
                }
            } 
        },
        { ConditionID.par, new Condition() 
            { 
                Name = "Parálisis", 
                StartMessage = "¡ha sido paralizado!" ,
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    if(Random.Range(1,5)==1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está paralizado y no puede moverse.");
                        return false;
                    }
                    return true;
                }
                
            } 
        },
        { ConditionID.frz, new Condition() 
            { 
                Name = "Congelado", 
               
                StartMessage = "¡ha sido congelado!",
                OnBeforeMove=(Pokemon pokemon)=>
                {
                    if(Random.Range(1,5)==1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} se ha descongelado.");
                        return true;
                    }
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está congelado y no puede moverse.");
                    return false;
                }
            }
        },
            { ConditionID.confusion, new Condition() 
                { 
                    Name = "Confusión", 
                    StartMessage = "¡está confundido!" ,
                    OnStart=(Pokemon pokemon)=>
                    {
                        pokemon.VolatileStatusTime = Random.Range(1,4);
                    },
                    OnBeforeMove=(Pokemon pokemon)=>
                    {
                        if(pokemon.VolatileStatusTime<=0)
                        {
                            pokemon.CureVolatileStatus();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} ya no está confundido.");
                            return true;
                        }
                        pokemon.VolatileStatusTime--;
                        if(Random.Range(1,3)==1)
                        {
                           return true;
                        }
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} está confundido");
                        pokemon.DecreaseHP(pokemon.MaxHp/8);
                        pokemon.StatusChanges.Enqueue("Se ha lastimado a si mismo por la confusión");
                        return false;

                    }
                }
            }

    };
    public static float GetStatusBonus(Condition condition)
    {
        if(condition==null) return 1f;
        else if(condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if(condition.Id == ConditionID.par|| condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;
        else
            return 1f;
    }
}
public enum ConditionID
{
   none, psn,brn,slp,par, frz,confusion
}
