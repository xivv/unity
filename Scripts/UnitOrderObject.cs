using UnityEngine;

public class UnitOrderObject : TurnOrderObject
{

    public Unit unit;
    public int rolledInit;

    protected override bool allowMovement(Vector2 targetCell)
    {
        return base.allowMovement(targetCell) && !hitsUnit(targetCell);
    }

    public override void BeforeTurn()
    {
        base.BeforeTurn();
        // if the unit is stunned we skip our turn
        if (unit.IsStunned()) this.canAct = false;
        this.remainingMovementSpeed = this.unit.encounterStats.speed;
    }

    public void RollInitiative()
    {
        this.rolledInit = (int)Random.Range(1, 20 + this.unit.encounterStats.init);
    }
}
