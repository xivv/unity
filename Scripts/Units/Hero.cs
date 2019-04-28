using System.Collections.Generic;

public abstract class Hero : Unit
{
    public List<HeroClass> heroClasses = new List<HeroClass>();

    public Hero(string unitName, MetaInformation metaInformation, Stats baseStats, List<HeroClass> heroClasses) : base(unitName, metaInformation, baseStats, TypeClass.HERO)
    {
        this.heroClasses = heroClasses;
    }

    public int getLevelsOfClass(HeroClass targetHeroClass)
    {
        int counter = 0;

        foreach (HeroClass heroClass in this.heroClasses)
        {
            if (heroClass == targetHeroClass) counter++;
        }

        return counter;
    }
}

public enum HeroClass
{
    CLERIC,
    BARBAR,
    WIZARD
}
