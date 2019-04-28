using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Encounter : MonoBehaviour
{

    public List<UnitOrderObject> participants = new List<UnitOrderObject>();
    private List<UnitOrderObject> monster = new List<UnitOrderObject>();
    private List<UnitOrderObject> heroes = new List<UnitOrderObject>();
    private List<UnitOrderObject> initiative = new List<UnitOrderObject>();

    // Camera Stuff
    public Camera unitCamera;
    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    [Range(0.5f, 2f)]
    public float smoothTime = 1f;

    public Tilemap groundTilemap;
    public Tilemap wallTilemap;

    // By fleeing we can destroy the turn cycle
    [HideInInspector]
    public bool alive;

    // TurnOrder
    private UnitOrderObject unitToAct;
    private int initiativeCounter = 0;
    public TargetSelector targetSelector;
    public AbilityMenu abilityMenu;

    // UI
    public Canvas canvas;
    public GameObject actButton;
    public GameObject defendActionButton;
    public GameObject finishSelectionButton;

    // Start is called before the first frame update
    void Start()
    {

        // Add units to initiative
        foreach (UnitOrderObject unitOrderObject in participants)
        {
            Unit unit = unitOrderObject.unit;

            if (!unit.isDead)
            {
                unitOrderObject.RollInitiative();
                initiative.Add(unitOrderObject);

                if (unit.typeClass == TypeClass.HERO)
                {
                    heroes.Add(unitOrderObject);
                }
                else if (unit.typeClass == TypeClass.MONSTER)
                {
                    monster.Add(unitOrderObject);
                }
            }
        }

        // Sort by init
        initiative.Sort((a, b) =>
        {
            if (a.rolledInit >= b.rolledInit)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        });

        // Initiate turn order
        alive = true;
        unitToAct = initiative[initiativeCounter];
        unitToAct.BeforeTurn();
        offset = unitCamera.transform.position - unitToAct.transform.position;
    }

    void ToggleActions()
    {
        bool unitCanActAndIsAbleToMove = this.unitToAct.unit.hasStandardAction && !this.unitToAct.pausedMovement;
        this.actButton.SetActive(unitCanActAndIsAbleToMove);
        this.defendActionButton.SetActive(unitCanActAndIsAbleToMove);
    }

    void RemoveDeadUnits()
    {
        List<UnitOrderObject> deadUnits = new List<UnitOrderObject>();

        foreach (UnitOrderObject unitOrderObject in this.participants)
        {
            if (unitOrderObject.unit.isDead)
            {
                deadUnits.Add(unitOrderObject);
            }
        }

        foreach (UnitOrderObject unitOrderObject in deadUnits)
        {
            RemoveDeadUnit(unitOrderObject);
        }
    }

    void RemoveDeadUnit(UnitOrderObject unitOrderObject)
    {
        this.participants.Remove(unitOrderObject);
        this.initiative.Remove(unitOrderObject);
        Destroy(unitOrderObject.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

        if (alive && bothPartiesLive())
        {
            RemoveDeadUnits();

            // If we selected an ability we grab it here and first activate the target selection
            if (this.abilityMenu.canAct == true && this.abilityMenu.selectedAbility != null)
            {
                this.abilityMenu.canAct = false;
                this.abilityMenu.gameObject.SetActive(false);
                Ability selectedAbility = this.abilityMenu.selectedAbility;
                SelectTargets(this.unitToAct.transform.position, selectedAbility.minTargets, selectedAbility.maxTargets, selectedAbility.reach);
            }

            // We enable - disable actions
            ToggleActions();

            // Apply Conditions
            if (!unitToAct.canAct)
            {
                initiativeCounter++;

                // If one turn ends and we need to start over again
                if (initiative.Count == initiativeCounter)
                {
                    initiativeCounter = 0;
                }

                // We give the unit all its previous state (movement etc.)
                unitToAct = initiative[initiativeCounter];
                CheckConditions(unitToAct.unit);
                unitToAct.BeforeTurn();
            }

            // If we are done with the selection we reactivate the units movement or resume to take action
            if (this.unitToAct.pausedMovement == true && this.targetSelector.pausedMovement == true && !this.abilityMenu.canAct)
            {

                // We use the ability and remove the standard action
                if (this.targetSelector.selectedTargets.Count > 0 && (this.targetSelector.selectedTargets.Count >= this.targetSelector.minTargets || this.targetSelector.minTargets == null))
                {

                    List<Unit> targets = new List<Unit>();
                    foreach (UnitOrderObject unitOrderObject in this.targetSelector.selectedTargets)
                    {
                        targets.Add(unitOrderObject.unit);
                    }

                    this.abilityMenu.selectedAbility.executeAbility(this.unitToAct.unit, targets);
                    this.targetSelector.selectedTargets.Clear();
                    this.unitToAct.pausedMovement = false;
                    this.abilityMenu.selectedAbility = null;
                    if (this.unitToAct.remainingMovementSpeed <= 0)
                    {
                        this.unitToAct.canAct = false;
                    }
                    else
                    {
                        this.unitToAct.unit.hasStandardAction = false;
                    }
                }
                // We cancle the action
                else
                {
                    Debug.Log("No targets selected.>");
                    this.unitToAct.pausedMovement = false;
                    this.abilityMenu.selectedAbility = null;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (alive)
        {

            TurnOrderObject observingUnit = this.unitToAct;

            if (this.unitToAct.pausedMovement == true)
            {
                observingUnit = this.targetSelector;
            }

            if (!observingUnit.canAct)
            {
                offset = unitCamera.transform.position - observingUnit.transform.position;
            }
            else
            {

                moveUnitCamera(observingUnit);
            }

        }
    }

    // Actions

    private void SelectTargets(Vector2 startingPosition, int? minTargets, int? maxTargets, int reach)
    {
        // If its an AOE Effect where we dont need to target anybody
        if (minTargets == null && maxTargets == null)
        {
            // Check the distance for everybody and set the list of the targetselector to the units we got
            foreach (UnitOrderObject unitOrderObject in this.participants)
            {
                float distance = Vector2.Distance(startingPosition, unitOrderObject.transform.position);
                if (distance <= reach + 1)
                {
                    this.targetSelector.selectedTargets.Add(unitOrderObject);
                }
            }
            this.finishSelection();
        }
        else
        {
            this.targetSelector.transform.position = startingPosition;
            this.targetSelector.gameObject.SetActive(true);
            this.unitToAct.pausedMovement = true;
            this.targetSelector.pausedMovement = false;
            this.actButton.SetActive(false);
            this.finishSelectionButton.SetActive(true);
            this.targetSelector.minTargets = minTargets;
            this.targetSelector.maxTargets = maxTargets;
            this.targetSelector.reach = reach;
            this.targetSelector.startingPosition = startingPosition;
        }
    }

    public void StartAbilitySelection()
    {
        // If the character does not have abilities
        if (this.unitToAct.unit.abilities.Count <= 0) return;

        // Stop the movement of the unit
        this.unitToAct.pausedMovement = true;

        // Show the panel for the abilities
        this.abilityMenu.gameObject.SetActive(true);
        // Add the abilities of the unit to the panel
        this.abilityMenu.setAbilities(this.unitToAct.unit.abilities);
        this.abilityMenu.canAct = true;
    }

    public void finishSelection()
    {
        this.targetSelector.pausedMovement = true;
        this.targetSelector.gameObject.SetActive(false);
        this.actButton.SetActive(true);
        this.finishSelectionButton.SetActive(false);
    }

    public void defendAction()
    {
        this.AddCondition(1, ConditionType.DEFENSE, unitToAct.unit, unitToAct.unit);
        // End the turn
        this.unitToAct.canAct = false;
    }

    public void fleeAction()
    {
        // Ends the encounter
        this.alive = false;
        this.unitToAct.canAct = false;
    }

    // Reset everything to starting state
    public void endTurnAction()
    {
        this.unitToAct.canAct = false;
        this.abilityMenu.gameObject.SetActive(false);
        this.abilityMenu.selectedAbility = null;
        this.targetSelector.pausedMovement = true;
        this.targetSelector.gameObject.SetActive(false);
        this.targetSelector.selectedTargets.Clear();
        this.actButton.SetActive(true);
        this.finishSelectionButton.SetActive(false);

    }

    // -----------------

    public void AddCondition(int duration, ConditionType conditionType, Unit source, Unit target)
    {
        target.conditions.Add(new Condition(conditionType, duration, source));
    }

    private void CheckConditions(Unit source)
    {
        foreach (UnitOrderObject unitOrderObject in this.participants)
        {

            Unit unit = unitOrderObject.unit;

            for (int i = unit.conditions.Count - 1; i >= 0; i--)
            {

                Condition condition = unit.conditions[i];

                // Reverse
                if (condition.source == source)
                {
                    condition.remainingTime -= 1;

                    if (condition.remainingTime <= 0)
                    {
                        unit.conditions.RemoveAt(i);

                        if (condition.conditionType == ConditionType.DEFENSE)
                        {
                            unit.encounterStats.ac -= 3;

                        }
                        else if (condition.conditionType == ConditionType.RAGE)
                        {
                            unit.encounterStats.strength -= 2;
                            unit.encounterStats.will += 1;
                        }
                        else if (condition.conditionType == ConditionType.FEARED)
                        {
                            unit.encounterStats.will += 2;
                        }
                        else if (condition.conditionType == ConditionType.POISONED)
                        {
                            unit.encounterStats.fortitude += 1;
                            unit.encounterStats.constitution += 2;
                        }
                        else if (condition.conditionType == ConditionType.BLINDED)
                        {
                            unit.encounterStats.strength += 1;
                            unit.encounterStats.dexterity += 1;
                        }
                        else if (condition.conditionType == ConditionType.ENTANGLED || condition.conditionType == ConditionType.HASTE)
                        {
                            unit.encounterStats.speed = unit.baseStats.speed;
                        }
                        else if (condition.conditionType == ConditionType.BLESSED)
                        {
                            unit.encounterStats.strength -= 1;
                            unit.encounterStats.dexterity -= 1;
                            unit.encounterStats.constitution -= 1;
                            unit.encounterStats.intelligence -= 1;
                            unit.encounterStats.wisdom -= 1;
                            unit.encounterStats.charisma -= 1;
                        }
                    }
                    // Ongoing buffs that do something every round
                    else
                    {
                        if (condition.conditionType == ConditionType.BLEEDING)
                        {
                            int damage = Convert.ToInt32(unit.encounterStats.health * 0.05) * -1;
                            unit.handleHealthChange(damage, DamageType.BLEED);
                        }
                        else if (condition.conditionType == ConditionType.POISONED)
                        {
                            int damage = Convert.ToInt32(unit.encounterStats.health * 0.02) * -1;
                            unit.handleHealthChange(damage, DamageType.POISON);
                        }
                        else if (condition.conditionType == ConditionType.REGENERATING)
                        {
                            int damage = Convert.ToInt32(unit.encounterStats.health * 0.05);
                            unit.handleHealthChange(damage, DamageType.HOLY);
                        }
                    }

                }
                // Apply
                else if (condition.remainingTime == condition.duration)
                {
                    if (condition.conditionType == ConditionType.DEFENSE)
                    {
                        unit.encounterStats.ac += 3;
                    }
                    else if (condition.conditionType == ConditionType.RAGE)
                    {
                        unit.encounterStats.strength += 2;
                        unit.encounterStats.will -= 1;
                    }
                    else if (condition.conditionType == ConditionType.FEARED)
                    {
                        unit.encounterStats.will -= 2;
                    }
                    else if (condition.conditionType == ConditionType.BLEEDING)
                    {
                        int damage = Convert.ToInt32(unit.encounterStats.health * 0.05) * -1;
                        unit.handleHealthChange(damage, DamageType.BLEED);
                    }
                    else if (condition.conditionType == ConditionType.POISONED)
                    {
                        int damage = Convert.ToInt32(unit.encounterStats.health * 0.02) * -1;
                        unit.handleHealthChange(damage, DamageType.POISON);
                        unit.encounterStats.fortitude -= 1;
                        unit.encounterStats.constitution -= 2;
                    }
                    else if (condition.conditionType == ConditionType.BLINDED)
                    {
                        unit.encounterStats.strength -= 1;
                        unit.encounterStats.dexterity -= 1;
                    }
                    else if (condition.conditionType == ConditionType.ENTANGLED)
                    {
                        unit.encounterStats.speed = 0;
                    }
                    else if (condition.conditionType == ConditionType.BLESSED)
                    {
                        unit.encounterStats.strength += 1;
                        unit.encounterStats.dexterity += 1;
                        unit.encounterStats.constitution += 1;
                        unit.encounterStats.intelligence += 1;
                        unit.encounterStats.wisdom += 1;
                        unit.encounterStats.charisma += 1;
                    }
                    else if (condition.conditionType == ConditionType.REGENERATING)
                    {
                        int damage = Convert.ToInt32(unit.encounterStats.health * 0.05);
                        unit.handleHealthChange(damage, DamageType.HOLY);
                    }
                    else if (condition.conditionType == ConditionType.HASTE)
                    {
                        unit.encounterStats.speed = unit.baseStats.speed * 2;
                    }
                }


            }

        }
    }

    private void moveUnitCamera(TurnOrderObject observingUnit)
    {
        Vector3 offsetPosition = observingUnit.transform.position + offset;
        unitCamera.transform.position = Vector3.SmoothDamp(unitCamera.transform.position, offsetPosition, ref velocity, smoothTime);
    }

    public bool monstersLive()
    {
        foreach (UnitOrderObject unitOrderObject in monster)
        {
            if (!unitOrderObject.unit.isDead)
            {
                return true;
            }
        }
        return false;
    }

    public bool heroesLive()
    {
        foreach (UnitOrderObject unitOrderObject in heroes)
        {
            if (!unitOrderObject.unit.isDead)
            {
                return true;
            }
        }
        return false;
    }

    public bool bothPartiesLive()
    {
        bool living = heroesLive() && monstersLive();

        if (!living)
        {
            this.unitToAct.canAct = false;
            this.alive = false;
            Debug.Log("One of the parties died. Game Over");
        }
        return living;
    }

}
