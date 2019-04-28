using System.Collections.Generic;

public class Cleric : Hero
{
    public Cleric(string unitName) : base("Cleric",
        new MetaInformation
        {
            level = 1,
            exp = 0
        },
        new Stats
        {
            health = 10,
            ac = 16,
            init = 0,
            speed = 4,

            bab = 1,

            strength = 1,
            dexterity = 0,
            constitution = 1,
            intelligence = 0,
            wisdom = 2,
            charisma = 0,

            fortitude = 0,
            reflex = 0,
            will = 1
        }
        , new List<HeroClass>(new HeroClass[] {
                HeroClass.CLERIC
            }))
    {
        AbilityEffect abilityEffect = new AbilityEffect(new List<Condition>(new Condition[] {
                new Condition(ConditionType.CURSED,2,this)
            }), TargetType.ENEMY, 10, "will", false, "wisdom");

        Ability ability = new Ability("Skelettons Curse", "Description", 1, 1, 1, new List<AbilityEffect>(
            new AbilityEffect[] {
                abilityEffect
            }
        ));
        this.abilities.Add(ability);
    }
}
