using System.Collections.Generic;

public class Barbar : Hero
{
    public Barbar(string unitName) : base("Barbar",
       new MetaInformation
       {
           level = 1,
           exp = 0
       },
       new Stats
       {
           health = 12,
           ac = 14,
           init = 1,
           speed = 6,

           bab = 1,

           strength = 2,
           dexterity = 0,
           constitution = 2,
           intelligence = 0,
           wisdom = 0,
           charisma = 0,

           fortitude = 1,
           reflex = 0,
           will = 0
       }
       , new List<HeroClass>(new HeroClass[] {
                HeroClass.BARBAR
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
