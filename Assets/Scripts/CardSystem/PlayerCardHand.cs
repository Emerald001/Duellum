using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCardHand : CardHand {
    [SerializeField] private float fadeThreshold;
    [SerializeField] private float cardViewMoveSpeed;
    [SerializeField] private float moveOverDistance;

    protected override void OnEnable() {
        base.OnEnable();

        BaseCardBehaviour.OnHoverEnter += SetCardsToMoveOver;
        BaseCardBehaviour.OnHoverExit += SetCardsBackToStandardPos;
        BaseCardBehaviour.OnClick += HandleCardClick;
        CardHandStateMachine.OnDismiss += LineOutCards;
        CardHandStateMachine.OnUse += RemoveSpecificCard;
    }
    protected override void OnDisable() {
        base.OnDisable();

        BaseCardBehaviour.OnHoverEnter -= SetCardsToMoveOver;
        BaseCardBehaviour.OnHoverExit -= SetCardsBackToStandardPos;
        BaseCardBehaviour.OnClick -= HandleCardClick;
        CardHandStateMachine.OnDismiss -= LineOutCards;
        CardHandStateMachine.OnUse -= RemoveSpecificCard;
    }

    private void Update() {
        CardHandStateMachine?.OnUpdate();

        if (Input.GetKeyDown(KeyCode.V))
            GiveCard(OwnerID);
    }

    protected override void LineOutCards() {
        SortCards();

        int numObjects = cardVisuals.Count;
        float arcAngle = Mathf.Min((numObjects - 1) * spacing, maxWidth);
        float angleIncrement = arcAngle == 0 ? 0 : arcAngle / (numObjects - 1);
        float startAngle = -arcAngle / 2f;

        for (int i = 0; i < cardVisuals.Count; i++) {
            CardAssetHolder card = cardVisuals[i];
            Card cardData = cards[i];

            card.Name.text = cardData.Name;
            card.Discription.text = cardData.Discription;
            card.Icon.sprite = cardData.Icon;

            float angle = startAngle + i * angleIncrement;
            float radianAngle = Mathf.Deg2Rad * angle;
            float x = radius * Mathf.Sin(radianAngle);
            float y = radius * Mathf.Cos(radianAngle);

            Vector3 position = transform.position + new Vector3(x, y, i * .01f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, position - transform.position);

            int index = i;
            card.cardBehaviour.ClearQueue();
            card.cardBehaviour.SetActionQueue(new List<Action>() {
                new DoMethodAction(() => card.cardBehaviour.CanInvoke = false),
                new ActionStack(
                    new MoveObjectAction(card.gameObject, cardSpawnMoveSpeed, position + new Vector3(0, -radius, 0)),
                    new RotateAction(card.gameObject, rotation.eulerAngles, cardRotationSpeed, .01f)
                ),
                new DoMethodAction(() => card.cardBehaviour?.SetValues(position + new Vector3(0, -radius, 0) + new Vector3(0, raisedAmount, 0), selectedPosition.position, uiCam, index))
            });
        }
    }

    private void SetCardsToMoveOver(BaseCardBehaviour raisedCard, System.Action actionForRaisedCard) {
        if (cardVisuals.Where(x => x.cardBehaviour.CanInvoke == false).ToList().Count > 0)
            return;

        bool hasReachedraisedCard = false;
        foreach (CardAssetHolder card in cardVisuals) {
            if (card.cardBehaviour == raisedCard) {
                actionForRaisedCard.Invoke();
                hasReachedraisedCard = true;
                continue;
            }

            if (!hasReachedraisedCard) {
                card.cardBehaviour.ClearQueue();
                card.cardBehaviour.SetActionQueue(new List<Action>() {
                    new MoveObjectAction(card.gameObject, cardViewMoveSpeed, card.cardBehaviour.StandardPosition + new Vector3(-moveOverDistance, 0, 0)),
                });
            }
            else {
                card.cardBehaviour.ClearQueue();
                card.cardBehaviour.SetActionQueue(new List<Action>() {
                    new MoveObjectAction(card.gameObject, cardViewMoveSpeed, card.cardBehaviour.StandardPosition + new Vector3(moveOverDistance, 0, 0)),
                });
            }
        }
    }

    private void SetCardsBackToStandardPos(BaseCardBehaviour raisedCard, System.Action actionForRaisedCard) {
        if (cardVisuals.Where(x => x.cardBehaviour.CanInvoke == false).ToList().Count > 0)
            return;

        foreach (CardAssetHolder card in cardVisuals) {
            if (card.cardBehaviour == raisedCard) {
                actionForRaisedCard.Invoke();
                continue;
            }

            card.cardBehaviour.ClearQueue();
            card.cardBehaviour.SetActionQueue(new List<Action>() {
                new MoveObjectAction(card.gameObject, cardViewMoveSpeed, card.cardBehaviour.StandardPosition),
            });
        }
    }

    private void HandleCardClick(BaseCardBehaviour card) {
        AbilityCard abilityCard = cards[card.Index] as AbilityCard;
        CardHandStateMachine.SetMachine(abilityCard);
    }
}
