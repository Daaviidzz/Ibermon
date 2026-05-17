using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// =====================================================================================
//  Helpers compartidos por los tests del Bloque 2 (Combate, Pokémon, Inventario).
//
//  El problema: PokemonBase, MoveBase y los items del juego son ScriptableObjects
//  que normalmente se rellenan desde el Inspector de Unity. En tests automatizados
//  no podemos usar el Inspector, así que creamos instancias en memoria e inyectamos
//  los campos privados vía reflection.
// =====================================================================================
public static class TestHelpers
{
    // -----------------------------------------------------------------------------------
    //  Setea un campo privado (público o privado) por nombre vía reflection.
    // -----------------------------------------------------------------------------------
    public static void SetField(object target, string fieldName, object value)
    {
        // Buscamos el campo en la clase y, si no lo encuentra, subimos por la
        // cadena de herencia (ItemBase, PokemonBase, etc. tienen muchos campos
        // privados que las subclases heredan).
        var tipo = target.GetType();
        FieldInfo field = null;
        while (tipo != null && field == null)
        {
            field = tipo.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            tipo = tipo.BaseType;
        }
        if (field == null)
            throw new System.Exception(
                $"Campo '{fieldName}' no encontrado en {target.GetType().Name} ni en sus clases base");
        field.SetValue(target, value);
    }

    // -----------------------------------------------------------------------------------
    //  Crea un PokemonBase mock listo para usar en tests, con stats configurables.
    //  Importante: learnableMoves siempre es una lista (puede estar vacía) para que
    //  el constructor de Pokemon no peta al llamar Init().
    // -----------------------------------------------------------------------------------
    public static PokemonBase CrearPokemonBase(
        string nombre = "TestMon",
        PokemonType type1 = PokemonType.Normal,
        PokemonType type2 = PokemonType.None,
        int maxHp = 50, int attack = 50, int defense = 50,
        int spAttack = 50, int spDefense = 50, int speed = 50,
        int expYield = 100, int catchRate = 100,
        GrowthRate growthRate = GrowthRate.Medio,
        List<LearnableMove> learnableMoves = null)
    {
        var pb = ScriptableObject.CreateInstance<PokemonBase>();
        SetField(pb, "name", nombre);
        SetField(pb, "type1", type1);
        SetField(pb, "type2", type2);
        SetField(pb, "maxHp", maxHp);
        SetField(pb, "attack", attack);
        SetField(pb, "defense", defense);
        SetField(pb, "spAttack", spAttack);
        SetField(pb, "spDefense", spDefense);
        SetField(pb, "speed", speed);
        SetField(pb, "expYield", expYield);
        SetField(pb, "catchRate", catchRate);
        SetField(pb, "growthRate", growthRate);
        SetField(pb, "learnableMoves", learnableMoves ?? new List<LearnableMove>());
        return pb;
    }

    // -----------------------------------------------------------------------------------
    //  Crea un MoveBase mock listo para usar en tests.
    // -----------------------------------------------------------------------------------
    public static MoveBase CrearMoveBase(
        string nombre = "TestMove",
        PokemonType type = PokemonType.Normal,
        int power = 50, int accuracy = 100, int pp = 25,
        MoveCategory category = MoveCategory.Fisico,
        int priority = 0, bool alwaysHit = false)
    {
        var mb = ScriptableObject.CreateInstance<MoveBase>();
        SetField(mb, "name", nombre);
        SetField(mb, "type", type);
        SetField(mb, "power", power);
        SetField(mb, "accuracy", accuracy);
        SetField(mb, "pp", pp);
        SetField(mb, "category", category);
        SetField(mb, "priority", priority);
        SetField(mb, "alwaysHit", alwaysHit);
        SetField(mb, "secondries", new List<SecondaryEffects>());
        return mb;
    }
}
