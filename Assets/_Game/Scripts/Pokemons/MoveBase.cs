using System.Collections.Generic;
using UnityEngine;

// Permite crear nuevos movimientos como archivos de datos (Assets) desde el menú de Unity.
[CreateAssetMenu(fileName = "Move", menuName = "Pokemons/New Move")]
public class MoveBase : ScriptableObject
{

    [SerializeField] string name;

    [TextArea] // Crea un cuadro de texto más grande en el Inspector para la descripción.
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;

    // Probabilidad de acierto
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHit; // Siempre golpea el movimiento

    // Puntos de Poder: cantidad de veces que se puede ejecutar este ataque.
    [SerializeField] int pp;

    // Clasificación: Físico, Especial o Estado 
    [SerializeField] MoveCategory category;

    // Efectos secundarios 
    [SerializeField] MoveEffects effects;

    // Define a quién golpea: al enemigo o al propio usuario (útil para 2vs2 en el futuro).
    [SerializeField] MoveTarget target;

    // --- PROPIEDADES (GETTERS) ---

    public string Name => name;
    public string Description => description;
    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool AlwaysHit => alwaysHit;
    public int Pp => pp;
    public MoveCategory Category => category;
    public MoveEffects Effects => effects;
    public MoveTarget Target => target;
}

[System.Serializable] // Permite que esta clase aparezca y sea editable dentro del Inspector de Unity.
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status; // Condición de estado que puede causar el movimiento (Envenenado, Paralizado, etc.)

    public List<StatBoost> Boosts => boosts;
    public ConditionID Status => status;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat; // La estadística que se ve afectada (Ataque, Defensa, etc.)
    public int boost; // El nivel de cambio (ej: +1, -1)
}


public enum MoveCategory
{
    Fisico,   // Basado en la estadística de Ataque.
    Especial, // Basado en la estadística de Ataque Especial.
    Estado    // Movimientos que no hacen daño directo, sino que alteran el combate.
}

public enum MoveTarget
{
    Foe,  // Objetivo: El enemigo.
    Self  // Objetivo: El propio Pokémon que usa el movimiento.
}