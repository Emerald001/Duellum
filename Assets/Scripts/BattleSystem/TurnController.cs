using System.Collections.Generic;
using UnityEngine;

public abstract class TurnController {
    public bool IsDone { get; protected set; }

    protected List<UnitController> units = new();
    protected UnitController currentUnit;
    protected bool isPicking = true;

    public virtual void OnEnter() {
        IsDone = false;

        bool hasUnitsLeft = false;
        foreach (var unit in units) {
            if (!UnitStaticManager.DeadUnitsInPlay.Contains(unit))
                hasUnitsLeft = true;
        }

        if (hasUnitsLeft) {
            EventManager<BattleEvents>.Invoke(BattleEvents.BattleEnd);
        }
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
            //GridStaticFunctions.ResetBattleTileColors();

            currentUnit = unit;
            currentUnit.OnEnter();
            isPicking = false;
        }
    }
}