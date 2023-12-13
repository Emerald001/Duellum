using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnController : TurnController {
    public CardHand CardHand { get; set; }

    public override void OnEnter() {
        base.OnEnter();

        // TO DO: Calculate unit action values, and pick the highest one
        List<KeyValuePair<int, UnitController>> unitActions = new();
        foreach (UnitController u in units)
            unitActions.Add(new((u as EnemyUnitController).PickEvaluatedAction(CardHand.AbilityCards), u));

        int value = 0;
        UnitController unit = null;
        foreach (var item in unitActions) {
            if (item.Key > value) {
                value = item.Key;
                unit = item.Value;
            }
        }

        PickUnit(UnitStaticManager.UnitPositions[unit]);
    }

    public override void OnExit() {
        base.OnExit();
    }

    public override void OnUpdate() {
        base.OnUpdate();
    }
}
