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
    [SerializeField] int expYield; // Experiencia que da el pokemon
    [SerializeField] GrowthRate growthRate; // Velocidad de crecimiento
    
    [SerializeField] int catchRate = 255;

    // Movimientos aprendibles
    [SerializeField] List<LearnableMove> learnableMoves;

    public int GetExpForLevel(int level) 
    {
        if (growthRate == GrowthRate.Rapido)
            return 4 * (level * level * level) / 5; // Experiencia que da el pokemon 
        else if (growthRate == GrowthRate.Medio)
            return level * level * level;
        return -1;
    }

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

    public int CatchRate=>catchRate;
    public int ExpYield => expYield; // Experiencia que da el pokemon
    public GrowthRate GrowthRate => growthRate;
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
    Fuego,
    Agua,
    Electrico,
    Planta,
    Hielo,
    Lucha,
    Veneno,
    Tierra,
    Volador,
    Psiquico,
    Bicho,
    Roca,
    Fantasma,
    Dragon
}

public class TypeChart
{
    //Matriz de efectividades
    static float[][] chart =
    {
        /*                      normal fuego agua electrico planta hielo lucha veneno tierra volador psiquico bicho roca fantasma dragon */
        /*Normal*/  new float[] {1f,   1f,   1f,  1f,       1f,    1f,   1f,   1f,    1f,    1f,     1f,       1f,  0.5f, 0f,     1f },
        /*Fuego*/   new float[] {1f,  0.5f, 0.5f, 1f,      2f,    2f,   1f,   1f,    1f,    1f,     1f,      2f,  0.5f, 1f,     0.5f},
        /*Agua*/    new float[] {1f,   2f,  0.5f, 1f,     0.5f,   1f,   1f,   1f,    2f,    1f,     1f,      1f,   2f, 1f,     0.5f},
        /*Electrico*/new float[] {1f,   1f,   2f, 0.5f,    0.5f,   1f,   1f,   1f,    0f,    2f,     1f,      1f,   1f, 1f,     0.5f},
        /*Planta*/  new float[] {1f,  0.5f,  2f, 1f,      0.5f,   1f,   1f,  0.5f,   2f,   0.5f,    1f,     0.5f, 2f, 1f,     0.5f},
        /*Hielo*/   new float[] {1f,  0.5f, 0.5f, 1f,      2f,    0.5f,  1f,   1f,    2f,    2f,     1f,      1f,   1f, 1f,     2f },
        /*Lucha*/   new float[] {2f,   1f,   1f, 1f,      1f,    2f,   1f,  0.5f,   1f,   0.5f,    2f,      0.5f, 2f, 0f,     1f },
        /*Veneno*/  new float[] {1f,   1f,   1f, 1f,      2f,    1f,   1f,  0.5f,  0.5f,   1f,     1f,      1f,  0.5f, 0.5f,   1f },
        /*Tierra*/  new float[] {1f,   2f,   1f, 2f,      0.5f,   1f,   1f,   2f,    1f,    0f,     1f,      2f,   1f, 1f,     1f },
        /*Volador*/ new float[] {1f,   1f,   1f, 0.5f,    2f,    1f,   2f,   1f,    1f,    1f,     1f,      2f,     0.5f, 1f,     1f },
        /*Psiquico*/new float[] {1f,   1f,   1f, 1f,      1f,    1f,   2f,   2f,    1f,    1f,    0.5f,     0.5f,   1f, 1f,     1f },
        /*Bicho*/   new float[] {1f,   0.5f, 1f, 1f,      1f,    1f,   0.5f, 0.5f,   1f,    0.5f,     2f,      1f,   1f, 0.5f,   1f },
        /*Roca*/    new float[] {1f,   2f,   1f, 1f,      1f,    2f,   0.5f, 1f,    0.5f,   2f,     1f,      2f,   1f, 1f,     1f },
        /*Fantasma*/new float[] {0f,   1f,   1f, 1f,      1f,    1f,   1f,   1f,    1f,    1f,     2f,      1f,   1f,  2f,     1f },
        /*Dragon*/  new float[] {1f,   1f,   1f, 1f,      1f,    1f,   1f,   1f,    1f,    1f,     1f,      1f,   1f,  1f,     2f }
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;
        // Ajustar índices para que coincidan con la matriz (restar 1)
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;
        return chart[row][col];
    }
}
public enum GrowthRate
{
    Medio,
    Rapido
}
public enum Stat
{
    Ataque,
    Defensa,
    AtaqueEspecial,
    DefensaEspecial,
    Velocidad

}