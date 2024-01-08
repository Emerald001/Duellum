using System.Collections.Generic;

public class EnemyTurnController : TurnController {
    public CardHand CardHand { get; set; }

    public override void OnEnter() {
        base.OnEnter();

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

        AbilityCard card = (unit as EnemyUnitController).CardToUse;
        if (card != null)
            CardHand.RemoveSpecificCard(card);

        PickUnit(UnitStaticManager.UnitPositions[unit]);
    }

    public override void OnExit() {
        base.OnExit();
    }

    public override void OnUpdate() {
        base.OnUpdate();
    }
}
