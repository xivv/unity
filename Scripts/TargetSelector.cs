using System.Collections.Generic;
using UnityEngine;

public class TargetSelector : TurnOrderObject
{
    [HideInInspector]
    public List<UnitOrderObject> selectedTargets = new List<UnitOrderObject>();
    [HideInInspector]
    public int? maxTargets;
    [HideInInspector]
    public int? minTargets;
    [HideInInspector]
    public int reach;
    [HideInInspector]
    public Vector2 startingPosition;

    protected override bool allowMovement(Vector2 targetCell)
    {
        float distance = Vector2.Distance(startingPosition, targetCell);
        return base.allowMovement(targetCell) && distance < this.reach + 1;
    }

    void OnGUI()
    {
        if (this.canAct && !this.pausedMovement)
        {
            if (Event.current.Equals(Event.KeyboardEvent(KeyCode.KeypadEnter.ToString())) || Event.current.Equals(Event.KeyboardEvent(KeyCode.Return.ToString())))
            {
                bool targetAquired = this.hitsUnit(this.transform.position);

                if (targetAquired)
                {

                    UnitOrderObject selectedUnit = this.rayCastToUnit(this.transform.position).transform.gameObject.GetComponent<UnitOrderObject>();

                    // Check if we already selected the target thus removing it
                    if (this.selectedTargets.Contains(selectedUnit))
                    {
                        this.selectedTargets.Remove(selectedUnit);
                    }

                    // If the list is not full already
                    else if (this.selectedTargets.Count < this.maxTargets)
                    {
                        this.selectedTargets.Add(selectedUnit);
                    }

                }

            }

            if (Event.current.Equals(Event.KeyboardEvent(KeyCode.Escape.ToString())))
            {
                this.selectedTargets.Clear();
                this.pausedMovement = true;
            }
        }
    }
}
