using System.Collections.Generic;
using UnityEngine;

public abstract class TurnController {
    public bool IsDone { get; protected set; }
    public int ID { get; protected set; }
    public List<UnitController> Units => units;

    protected List<UnitController> units = new();
    protected UnitController currentUnit;
    protected bool isPicking = true;

    public void SetUp(int id, List<UnitController> units) {
        this.units = units;
        ID = id;
    }

    public virtual void OnEnter() {
        IsDone = false;
    }

    public virtual void OnUpdate() {
        if (isPicking)
            return;
        EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.CameraToCurrentUnit, currentUnit);
        currentUnit?.OnUpdate();
        if (currentUnit.IsDone) {
            IsDone = true;
        }
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
            currentUnit = unit;
            currentUnit.OnEnter();
            isPicking = false;
        }
    }
}