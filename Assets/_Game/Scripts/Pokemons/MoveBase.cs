using System;
using System.Collections.Generic;
using UnityEngine;

// Permite crear nuevos movimientos como archivos de datos (Assets) desde el men� de Unity.
[CreateAssetMenu(fileName = "Move", menuName = "Pokemons/New Move")]
public class MoveBase : ScriptableObject
{

    [SerializeField] new string name;

    [TextArea] // Crea un cuadro de texto m�s grande en el Inspector para la descripci�n.
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;

    // Probabilidad de acierto
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHit; // Siempre golpea el movimiento

    // Puntos de Poder: cantidad de veces que se puede ejecutar este ataque.
    [SerializeField] int pp;
    [SerializeField] int priority;

    // Clasificaci�n: F�sico, Especial o Estado 
    [SerializeField] MoveCategory category;

    // Efectos secundarios 
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondries;


    // Define a qui�n golpea: al enemigo o al propio usuario (�til para 2vs2 en el futuro).
    [SerializeField] MoveTarget target;

    // --- PROPIEDADES (GETTERS) ---

    public string Name => name;
    public string Description => description;
    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool AlwaysHit => alwaysHit;
    public int PP => pp;
    public int Priority => priority;
    public MoveCategory Category => category;
    public MoveEffects Effects => effects;
    public MoveTarget Target => target;
    public List<SecondaryEffects> Secondries => secondries;
}

[System.Serializable] // Permite que esta clase aparezca y sea editable dentro del Inspector de Unity.
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status; // Condici�n de estado que puede causar el movimiento (Envenenado, Paralizado, etc.)
    [SerializeField] ConditionID volatileStatus;

    public List<StatBoost> Boosts => boosts;
    public ConditionID Status => status;
    public ConditionID VolatileStatus => volatileStatus;
}
[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;
    public int Chance => chance;
    public MoveTarget Target => target;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat; // La estad�stica que se ve afectada (Ataque, Defensa, etc.)
    public int boost; // El nivel de cambio (ej: +1, -1)
}


public enum MoveCategory
{
    Fisico,   // Basado en la estad�stica de Ataque.
    Especial, // Basado en la estad�stica de Ataque Especial.
    Estado    // Movimientos que no hacen da�o directo, sino que alteran el combate.
}

public enum MoveTarget
{
    Foe,  // Objetivo: El enemigo.
    Self  // Objetivo: El propio Pok�mon que usa el movimiento.
}