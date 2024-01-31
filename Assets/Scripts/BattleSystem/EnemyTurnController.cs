using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTurnController : TurnController {
    public CardHand CardHand { get; set; }

    private ActionQueue actionQueue = new();
    private AbilityCard cardToUse;

    public override void OnEnter() {
        base.OnEnter();

        List<KeyValuePair<int, UnitController>> unitActions = new();
        foreach (UnitController u in units)
            unitActions.Add(new((u as EnemyUnitController).PickEvaluatedAction(CardHand.Cards), u));

        int value = 0;
        UnitController pickedUnit = null;
        foreach (var item in unitActions) {
            if (item.Key > value) {
                value = item.Key;
                pickedUnit = item.Value;
            }
        }

        AbilityCard card = (pickedUnit as EnemyUnitController).CardToUse;
        Vector2Int targetPos = GridStaticFunctions.CONST_EMPTY;
        var otherCards = CardHand.Cards.Where(x => x.CardType == CardType.Action || x.CardType == CardType.UnitSpecific).ToList();

        if (card != null) {
            cardToUse = card;
            targetPos = UnitStaticManager.GetUnitPosition(pickedUnit);
        }
        else if (otherCards.Count > 0) {
            cardToUse = otherCards[0] as AbilityCard;

            UnitController enemyUnit = UnitStaticManager.GetEnemies(ID)[Random.Range(0, UnitStaticManager.GetEnemies(ID).Count)];
            targetPos = UnitStaticManager.GetUnitPosition(enemyUnit);
        }

        if ((CardHand as EnemyCardHand).CanUseCard(cardToUse)) {
            actionQueue.Enqueue(new WaitAction(.5f));
            actionQueue.Enqueue(new DoMethodAction(() => (CardHand as EnemyCardHand).DisplayCard(cardToUse)));
            actionQueue.Enqueue(new WaitAction(2f));
            actionQueue.Enqueue(new DoMethodAction(() => CardHand.OnPickPosition?.Invoke(targetPos)));
        }

        actionQueue.Enqueue(new WaitAction(.5f));
        actionQueue.Enqueue(new DoMethodAction(() => PickUnit(UnitStaticManager.UnitPositions[pickedUnit])));

    }

    public override void OnExit() {
        base.OnExit();
    }

    public override void OnUpdate() {
        base.OnUpdate();

        actionQueue.OnUpdate();
    }
}
