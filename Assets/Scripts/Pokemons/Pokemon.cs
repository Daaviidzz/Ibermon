using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//Clase principal de Pokemon que contiene los datos y metodos necesarios para el funcionamiento del mismo
//System.Serializable para que pueda ser serializado por Unity y guardado en archivos
[System.Serializable]
public class Pokemon
{
   [SerializeField] PokemonBase _base;
   [SerializeField] int level;

    public PokemonBase Base { get { 
            return _base;
        } }
    public int Level {
        get {  return level; }
            }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public void Init()
    {
        HP = MaxHp;

        Moves = new List<Move>();
        //Aprender movimientos segun el nivel
        foreach (var learnableMove in Base.LearnableMoves)
        {
            if (learnableMove.Level <= Level)
            {
                Moves.Add(new Move(learnableMove.MoveBase));
            }
            //solo puede aprender 4 movimientos
            if (Moves.Count >= 4)
                break;
        }
    }

    //Calculo de stats
    public int Attack
    {
        //Formula para calcular el stat de ataque(en el juego real es exactamente esta)
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }
    public int MaxHp
    {
        //Formula para calcular el stat de vida(en el juego real es exactamente esta)
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10; }
    }

    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        //Calculo de daño mas critico y efectividad 
        //critico 6.25% de probabilidad
        float critical = 1f;
        if (Random.value*100f<=6.25f)
        {
            critical = 2f;
        }

        //efectividad
        float type =TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Critical = critical,
            TypeEffectiveness = type,
            Fainted = false
        };

        //calculo de ataque y defensa especial o fisico
        float attack = (move.Base.IsSpecial)? attacker.SpAttack : attacker.Attack;
         float defense = (move.Base.IsSpecial)? this.SpDefense : this.Defense;

        //formulacion de daño
        float modifiers =Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if(HP <= 0) 
        {  
            HP = 0;
             damageDetails.Fainted = true;
        }
        return damageDetails;

    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);

        return Moves[r];
    }
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float TypeEffectiveness { get; set; }
    public float Critical { get; set; }
}
