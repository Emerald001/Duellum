using System.Collections.Generic;
using UnityEngine;

public abstract class TurnController {
    public bool IsDone { get; protected set; }

    protected List<UnitController> units;
    protected UnitController currentUnit;
    protected bool isPicking = true;

    public virtual void OnEnter() {
        IsDone = false;
    }

    public virtual void OnUpdate() {
        if (isPicking)
            return;

        currentUnit?.OnUpdate();
        if (currentUnit.IsDone)
            IsDone = true;
    }

    public virtual void OnExit() {
        currentUnit?.OnExit();
        currentUnit = null;

        IsDone = false;
        isPicking = true;
    }

    protected virtual void PickUnit(Vector2Int unitPosition) {
        if (!UnitStaticManager.TryGetUnitFromGridPos(unitPosition, out var unit))
            return;

        if (UnitStaticManager.DeadUnitsInPlay.Contains(unit))
            return;

        if (units.Contains(unit)) {
            GridStaticFunctions.ResetTileColors();

            currentUnit = unit;
            currentUnit.OnEnter();
            isPicking = false;
        }
    }
}