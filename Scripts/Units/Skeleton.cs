using System.Collections.Generic;

public class Skeleton : Unit
{
    public Skeleton(string unitName, MetaInformation metaInformation, Stats baseStats, TypeClass typeClass) : base("Skeleton",
       new MetaInformation
       {
           level = 1,
           exp = 0
       },
        new Stats
        {
            health = 4,
            ac = 12,
            init = -2,
            speed = 4,

            bab = 1,

            strength = 1,
            dexterity = 0,
            constitution = 1,
            intelligence = 0,
            wisdom = 0,
            charisma = 0,

            fortitude = 0,
            reflex = 0,
            will = 0
        }
       , TypeClass.MONSTER)
    {
        AbilityEffect abilityEffect = new AbilityEffect(new List<Condition>(new Condition[] {
                new Condition(ConditionType.BLEEDING,2,this)
            }), TargetType.ENEMY, 10, "fortitude", false, "constitution");

        Ability ability = new Ability("Skelettons Curse", "Description", 1, 1, 1, new List<AbilityEffect>(
            new AbilityEffect[] {
                abilityEffect
            }
        ));
        this.abilities.Add(ability);
    }
}
