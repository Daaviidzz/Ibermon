using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemons/New Pokemon")]
public class PokemonBase : ScriptableObject
{
    // Datos básicos
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    // Sprites
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    // Tipos
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Stats base
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    // Movimientos aprendibles
    [SerializeField] List<LearnableMove> learnableMoves;

    // Getters correctos (conectados a los campos serializados)
    public string Name => name;
    public string Description => description;
    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;
    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;
    public int MaxHp => maxHp;
    public int Attack => attack;
    public int Defense => defense;
    public int SpAttack => spAttack;
    public int SpDefense => spDefense;
    public int Speed => speed;
    public List<LearnableMove> LearnableMoves => learnableMoves;
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
