using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Tavern : MonoBehaviour
{

    List<Hero> heroesAvailable = new List<Hero>();
    public int maxHeroesAvailable = 4;
    public Text[] texts;

    // Start is called before the first frame update
    void Start()
    {
        generateNewHeroes();
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < maxHeroesAvailable; i++)
        {
            texts[i].text = heroesAvailable[i].heroClasses[0].ToString();
        }
    }

    public void generateNewHeroes()
    {
        for (var i = 0; i < maxHeroesAvailable; i++)
        {
            int random = Random.Range(0, Enum.GetNames(typeof(HeroClass)).Length);
            heroesAvailable.Add(generateRandomHero((HeroClass)random));
        }
    }

    public Hero generateRandomHero(HeroClass heroClass)
    {

        if (heroClass == HeroClass.BARBAR)
        {
            return new Barbar("Herbert");
        }
        else if (heroClass == HeroClass.CLERIC)
        {
            return new Cleric("Ghoran");
        }
        else if (heroClass == HeroClass.WIZARD)
        {
            return new Wizard("Merlin");
        }
        else
        {
            return null;
        }
    }
}
