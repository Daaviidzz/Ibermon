using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Pokemons
{
  public PokemonBase Base { get; set; }
    public int Level { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public Pokemons(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        Level = pLevel;
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
}
