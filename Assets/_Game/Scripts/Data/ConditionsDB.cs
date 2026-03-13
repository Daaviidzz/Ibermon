using System.Collections.Generic;
using UnityEngine;

//Esto luego seria una base de datos, pero por ahora lo dejo asi para probar el sistema de estados
public class ConditionsDB 
{
   
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>() 
    { 
        { ConditionID.poison, new Condition() 
            { 
                Name = "Veneno", 
                StartMessage = "¡ha sido envenenado!",
                OnAfterTurn=(Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} sufre daño por el veneno.");
                }
            } 
        },
        { ConditionID.burn, new Condition() 
            { 
                Name = "Quemadura", 
                StartMessage = "¡ha sido quemado!" ,
                OnAfterTurn=(Pokemon pokemon) =>
                {
                    pokemon.UpdateHP(pokemon.MaxHp/16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} sufre daño por la quemadura.");
                }
            } 
        },
        { ConditionID.sleep, new Condition() 
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
        { ConditionID.paralysis, new Condition() 
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
        { ConditionID.frozen, new Condition() 
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
        }
    };
}
public enum ConditionID
{
   none, poison,burn,sleep,paralysis, frozen
}
