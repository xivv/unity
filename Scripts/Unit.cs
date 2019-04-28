using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Unit
{
    /** Encounter Variables **/
    [HideInInspector]
    public String unitName;
    [HideInInspector]
    public MetaInformation metaInformation;
    [HideInInspector]
    public Stats baseStats;
    public Stats encounterStats;

    [HideInInspector]
    public List<Ability> abilities;

    // ----------------
    [HideInInspector]
    public TypeClass typeClass;
    public List<DamageType> resistances;
    public List<DamageType> weaknesses;
    public List<Condition> conditions;
    // ----------------

    [HideInInspector]
    public bool isDead = false;
    public bool hasStandardAction = true;

    public Unit(String unitName, MetaInformation metaInformation, Stats baseStats, TypeClass typeClass)
    {
        this.unitName = unitName;
        this.metaInformation = metaInformation;
        this.baseStats = baseStats;
        this.encounterStats = this.baseStats;
        this.typeClass = typeClass;
        this.abilities = new List<Ability>();
    }

    public bool IsStunned()
    {
        foreach (Condition condition in this.conditions)
        {
            if (condition.conditionType == ConditionType.STUNNED)
            {
                return true;
            }
        }
        return false;
    }

    public void handleHealthChange(int change, DamageType damageType)
    {
        if (this.resistances.Contains(damageType))
        {
            change = (int)Math.Floor((decimal)(change / 2));
        }
        else if (this.weaknesses.Contains(damageType))
        {
            change = change * 2;
        }
        else
        {
            this.encounterStats.health += change;
        }

        if (this.encounterStats.health <= 0)
        {
            Debug.Log(this.unitName + " died.");
            this.isDead = true;
        }
    }
}

[System.Serializable]
public class Condition
{
    public ConditionType conditionType;
    public int remainingTime;
    public int duration;
    public Unit source;

    public Condition(ConditionType conditionType, int duration, Unit source)
    {
        this.conditionType = conditionType;
        this.remainingTime = duration;
        this.duration = duration;
        this.source = source;
    }

}
public struct MetaInformation
{
    public int level;
    public int exp;
}

[System.Serializable]
public struct Stats
{
    public int health;
    public int ac;
    public int init;
    public int speed;

    public int bab;

    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public int wisdom;
    public int charisma;

    public int fortitude;
    public int reflex;
    public int will;
}

public enum ConditionType
{
    RAGE,
    FEARED,
    BLEEDING,
    POISONED,
    BLINDED,
    STUNNED,
    ENTANGLED,
    BLESSED,
    REGENERATING,
    HASTE,
    CURSED,
    DEFENSE
}

public enum DamageType
{
    UNTYPED,
    BLEED,
    ACID,
    FIRE,
    COLD,
    ENERGY,
    HOLY,
    UNHOLY,
    MAGIC,
    POISON
}

public enum TargetType
{
    ALLY,
    ENEMY,
    ALL
}

public enum TypeClass
{
    HERO,
    MONSTER,
    TARGETSELECTOR,
    ALL
}

public class AbilityEffect
{
    // Conditions
    public List<Condition> conditions = new List<Condition>();
    public TargetType targetType;
    public int? dc;
    public string savingThrow;

    // Damage
    public int? damageDie;
    public int? damageDice;
    public DamageType damageType;
    public bool confirmHit;

    // Bonus on either DC of ability or to hit
    public string abilityScoreBonus;

    public AbilityEffect(List<Condition> conditions, TargetType targetType, int? dc, string savingThrow, bool confirmHit, string abilityScoreBonus)
    {
        this.conditions = conditions;
        this.targetType = targetType;
        this.dc = dc;
        this.savingThrow = savingThrow;
        this.confirmHit = confirmHit;
        this.abilityScoreBonus = abilityScoreBonus;
    }

    public AbilityEffect(TargetType targetType, int damageDie, int damageDice, DamageType damageType, bool confirmHit, string abilityScoreBonus)
    {
        this.targetType = targetType;
        this.damageDie = damageDie;
        this.damageDice = damageDice;
        this.damageType = damageType;
        this.confirmHit = confirmHit;
        this.abilityScoreBonus = abilityScoreBonus;
    }

    public AbilityEffect(List<Condition> conditions, TargetType targetType, int? dc, string savingThrow, int damageDie, int damageDice, DamageType damageType, bool confirmHit, string abilityScoreBonus)
    {
        this.conditions = conditions;
        this.targetType = targetType;
        this.dc = dc;
        this.savingThrow = savingThrow;
        this.damageDie = damageDie;
        this.damageDice = damageDice;
        this.damageType = damageType;
        this.confirmHit = confirmHit;
        this.abilityScoreBonus = abilityScoreBonus;
    }

    public int rollDie(int? dice, int? die)
    {
        if (die < 0)
        {
            dice *= -1;
        }
        return (int)Random.Range((float)dice, (float)die);
    }

    public int rollDamage()
    {
        return rollDie(damageDice, damageDie);
    }

    public bool hitSucceded(Unit source, Unit target)
    {
        return this.rollDie(1, 20) + (int)typeof(Stats).GetField(this.abilityScoreBonus).GetValue(source.encounterStats) >=
            target.encounterStats.ac + target.encounterStats.dexterity;
    }

    public bool SavingThrowSucceded(Unit source, Unit target)
    {
        if (this.dc == null) return false;
        return this.rollDie(1, 20) + (int)typeof(Stats).GetField(this.savingThrow).GetValue(target.encounterStats) >=
            this.dc + (int)typeof(Stats).GetField(this.abilityScoreBonus).GetValue(source.encounterStats);
    }
}

public class Ability
{
    public string name;
    public string description;
    public int? minTargets; // If null we do AOE
    public int? maxTargets; // If null we do AOE
    public int reach;

    List<AbilityEffect> effects = new List<AbilityEffect>();

    public Ability(string name, string description, int? minTargets, int? maxTargets, int reach, List<AbilityEffect> effects)
    {
        this.name = name;
        this.description = description;
        this.minTargets = minTargets;
        this.maxTargets = maxTargets;
        this.reach = reach;
        this.effects = effects;
    }

    public void executeAbility(Unit source, List<Unit> targets)
    {
        foreach (Unit target in targets)
        {
            foreach (AbilityEffect effect in effects)
            {
                // If we cannot target this unit    
                if (effect.targetType == TargetType.ALLY && source.typeClass != target.typeClass)
                {
                    Debug.Log(target.unitName + ":> cant be targeted by ability because target is not an ALLY");
                    break;
                }
                else if (effect.targetType == TargetType.ENEMY && source.typeClass == target.typeClass)
                {
                    Debug.Log(target.unitName + ":> cant be targeted by ability because target is not an ENEMY");
                    break;
                }

                // If the ability needs to hit first
                if (effect.confirmHit)
                {
                    if (!effect.hitSucceded(source, target))
                    {
                        Debug.Log(target.unitName + ":> did not get hit by ability");
                        break;
                    }
                    else
                    {
                        target.handleHealthChange(effect.rollDamage(), effect.damageType);
                        Debug.Log(target.unitName + ":> did get hit by the ability");
                    }
                }
                else if (effect.damageDice != null && effect.damageDie != null)
                {
                    target.handleHealthChange(effect.rollDamage(), effect.damageType);
                    Debug.Log(target.unitName + ":> takes damage automatically");
                }

                // We first check if we need to do a saving throw
                if (effect.dc != null)
                {
                    if (effect.SavingThrowSucceded(source, target))
                    {
                        Debug.Log(target.unitName + ":> saving throw succeded");
                        break;
                    }
                    else
                    {
                        Debug.Log(target.unitName + ":> saving throw failed");

                        if (effect.conditions.Count > 0)
                        {
                            foreach (Condition condition in effect.conditions)
                            {

                                bool alreadyHasCondition = false;

                                // We check first if the character already has the condition and if yes we try to make the duration longer
                                foreach (Condition targetCondition in target.conditions)
                                {
                                    if (targetCondition.conditionType == condition.conditionType)
                                    {
                                        targetCondition.duration += condition.duration;
                                        targetCondition.remainingTime += condition.duration;
                                        Debug.Log(target.unitName + ":> already has condition " + condition.ToString());
                                        alreadyHasCondition = true;
                                    }
                                }

                                if (!alreadyHasCondition)
                                {
                                    target.conditions.Add(condition);
                                    Debug.Log(target.unitName + ":> new condition " + condition.ToString());
                                }
                            }
                        }

                    }
                }


            }
        }
    }
}
