using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Pokemons
{
  PokemonBase _base;
    int level;
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public Pokemons(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        HP = _base.MaxHp;

        Moves = new List<Move>();
        //Aprender movimientos segun el nivel
        foreach (var learnableMove in _base.LearnableMoves)
        {
            if (learnableMove.Level <= level)
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
        get { return Mathf.FloorToInt((_base.Attack * level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((_base.Defense * level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((_base.SpAttack * level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((_base.SpDefense * level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((_base.Speed * level) / 100f) + 5; }
    }
    public int MaxHp
    {
        //Formula para calcular el stat de vida(en el juego real es exactamente esta)
        get { return Mathf.FloorToInt((_base.MaxHp * level) / 100f) + 10; }
    }
}
