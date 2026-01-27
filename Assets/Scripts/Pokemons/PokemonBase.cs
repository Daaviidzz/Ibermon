using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//Para crear un nuevo asset de tipo PokemonBase desde el menu de Unity
[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemons/New Pokemon")]
public class PokemonBase : ScriptableObject
{
    //Datos basicos del pokemon
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;  

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //Base stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    //sp=special
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    //Property getters
    public string Name { get; set;}
    public string Description { get; }
    public Sprite FrontSprite { get; }
    public Sprite BackSprite { get; }
    public PokemonType Type1 { get; }
    public PokemonType Type2 { get; }
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int SpAttack { get; }
    public int SpDefense { get; }
    public int Speed { get; }
    public List<LearnableMove> LearnableMoves { get { return learnableMoves; } }

}
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;
    public MoveBase MoveBase { get { return moveBase; } }
    public int Level { get { return level; } }
}


//Tipos de pokemon
public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}
