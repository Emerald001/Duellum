using System.Collections.Generic;
using UnityEngine;

public class EnemyCardHand : CardHand {
    protected override void OnEnable() {
        base.OnEnable();

        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, ResetAfterBattle);
    }
    protected override void OnDisable() {
        base.OnDisable();

        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, ResetAfterBattle);
    }

    // TODO: Make this arc the other way and show the back of the cards!
    protected override void LineOutCards() {
        SortCards();

        int numObjects = cardVisuals.Count;
        float arcAngle = -Mathf.Min((numObjects - 1) * spacing, maxWidth);
        float angleIncrement = arcAngle == 0 ? 0 : arcAngle / (numObjects - 1);
        float startAngle = arcAngle / 2f;

        for (int i = 0; i < cardVisuals.Count; i++) {
            CardAssetHolder card = cardVisuals[i];

            float angle = startAngle - i * angleIncrement;
            float radianAngle = Mathf.Deg2Rad * angle;
            float x = radius * -Mathf.Sin(radianAngle);
            float y = radius * -Mathf.Cos(radianAngle);

            Vector3 position = transform.position + new Vector3(x, y, i * .01f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, position - transform.position);

            int index = i;
            card.cardBehaviour.ClearQueue();
            card.cardBehaviour.SetActionQueue(new List<Action>() {
                new DoMethodAction(() => card.cardBehaviour.CanInvoke = false),
                new ActionStack(
                    new MoveObjectAction(card.gameObject, cardSpawnMoveSpeed, position + new Vector3(0, radius, 0)),
                    new RotateAction(card.gameObject, rotation.eulerAngles, cardRotationSpeed, .01f)
                ),
                new DoMethodAction(() => card.cardBehaviour.SetValues(position + new Vector3(0, radius, 0) + new Vector3(0, -raisedAmount, 0), uiCam, index))
            });
        }
    }

    private void ResetAfterBattle() {
        for (int i = cardVisuals.Count - 1; i >= 0; i--)
            RemoveCard(i);

        cardVisuals.Clear();
        cardStack.ResetDeck();
    }
}
