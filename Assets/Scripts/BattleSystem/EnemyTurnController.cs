using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnController : TurnController {
    public override void OnEnter() {
        base.OnEnter();

        units = UnitStaticManager.EnemyUnitsInPlay;

        // TO DO: Calculate unit action values, and pick the highest one
        Dictionary<int, UnitController> unitActions = new();

        //foreach (UnitController u in units) {
        //    unitActions.Add(u as EnemyUnitInterface.PickEvaluatedAction(), u);
        //}

        int unit = Random.Range(0, units.Count);
        PickUnit(UnitStaticManager.UnitPositions[units[unit]]);
    }

    public override void OnExit() {
        base.OnExit();
    }

    public override void OnUpdate() {
        base.OnUpdate();
    }
}
