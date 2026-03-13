using System.Collections.Generic;
using UnityEngine;

// Clase principal que representa una instancia individual de un Pokémon.
[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
    }

    // Propiedades básicas
    public int Exp { get; set; }
    public PokemonBase Base => _base;
    public int Level => level;
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    // Diccionarios para gestionar estadísticas base y modificadores de combate (Evasión, Ataque, etc.)
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatsBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; } // Duración restante de la condición de estado, si es aplicable.

    // Cola de mensajes para notificar cambios de estado o buffs en la interfaz.
    public Queue<string> StatusChanges { get; private set; } 
    public bool HpChanged { get; set; }

    public void Init()
    {
        Moves = new(); 

        // El Pokémon aprende movimientos de su lista base según su nivel actual.
        foreach (var learnableMove in Base.LearnableMoves)
        {
            if (learnableMove.Level <= Level)
                Moves.Add(new Move(learnableMove.MoveBase));

            if (Moves.Count >= 4) break; // Límite clásico de 4 movimientos.
        }
        Exp=Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;
        StatusChanges = new();
        ResetStatBoost();
    }

    // Calcula las estadísticas finales basadas en la fórmula oficial de los juegos de Pokémon.
    void CalculateStats()
    {
        Stats = new();
        Stats.Add(Stat.Ataque, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defensa, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.AtaqueEspecial, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.DefensaEspecial, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Velocidad, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
    }

    // Reinicia los niveles de modificadores (buffs/debuffs) a 0.
    void ResetStatBoost()
    {
        StatsBoosts = new()
        {
            { Stat.Ataque, 0 },
            { Stat.Defensa, 0 },
            { Stat.AtaqueEspecial, 0 },
            { Stat.DefensaEspecial, 0 },
            { Stat.Velocidad, 0 },
            {Stat.Accuracy, 0 },
            {Stat.Evasion, 0}
        };
    }

    // Obtiene el valor real de una estadística aplicando el modificador actual (boost).
    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        int boost = StatsBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        // Si el boost es positivo multiplica, si es negativo divide.
        return boost >= 0
            ? Mathf.FloorToInt(statVal * boostValues[boost])
            : Mathf.FloorToInt(statVal / boostValues[-boost]);
    }

    // Aplica cambios a las estadísticas 
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            // Clampeamos entre -6 y 6 niveles (límite estándar).
            StatsBoosts[stat] = Mathf.Clamp(StatsBoosts[stat] + boost, -6, 6);

            string changeType = boost > 0 ? "aumento!" : "disminuyo!";
            StatusChanges.Enqueue($"{stat} de {Base.Name} {changeType}");
        }
    }

    // Getters 
    public int Attack => GetStat(Stat.Ataque);
    public int Defense => GetStat(Stat.Defensa);
    public int SpAttack => GetStat(Stat.AtaqueEspecial);
    public int SpDefense => GetStat(Stat.DefensaEspecial);
    public int Speed => GetStat(Stat.Velocidad);
    public int MaxHp { get; private set; }

    // Procesa el daño recibido por un ataque.
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        // Probabilidad de golpe crítico (6.25%).
        float critical = (Random.value * 100f <= 6.25f) ? 2f : 1f;

        // Cálculo de efectividad de tipos 
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            Critical = critical,
            TypeEffectiveness = type,
            Fainted = false
        };

        // Seleccionamos ataque y defensa según la categoría del movimiento.
        float attack = (move.Base.Category == MoveCategory.Especial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Especial) ? this.SpDefense : this.Defense;

        // Implementación de la fórmula de daño oficial.
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

       UpdateHP(damage);

        return damageDetails;
    }
    public void UpdateHP(int damage)
    {
        HP=Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }
    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return; // No se puede aplicar un nuevo estado si ya hay uno activo.
        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
    }
    public void CureStatus()=> Status = null;
    public Move GetRandomMove() => Moves[Random.Range(0, Moves.Count)];
    public bool OnBeforeMove() 
    {
        if (Status?.OnBeforeMove != null)
            return Status.OnBeforeMove.Invoke(this);
        return true;
    } 
    public void OnAfterTurn() => Status?.OnAfterTurn?.Invoke(this);
    public void OnBattleOver() => ResetStatBoost();

    //Restaura la vida
    public void ResetHealth()
    {
        HP=MaxHp;
    }
    //Comprueba para subir de nivel al pokemon
    public bool CheckForLevelUp()
    {
        if(Exp>=Base.GetExpForLevel(level+1))
        {
         ++level;
            return true;
        }
        return false;
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float TypeEffectiveness { get; set; }
    public float Critical { get; set; }
}