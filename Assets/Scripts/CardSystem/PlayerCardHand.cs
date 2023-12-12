using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCardHand : CardHand {
    [SerializeField] private float fadeThreshold;
    [SerializeField] private float cardViewMoveSpeed;
    [SerializeField] private float moveOverDistance;

    private bool hasCardFaded;
    private bool hasCardFadedCallRan;

    protected override void OnEnable() {
        base.OnEnable();

        BaseCardBehaviour.OnHoverEnter += SetCardsToMoveOver;
        BaseCardBehaviour.OnHoverExit += SetCardsBackToStandardPos;
        BaseCardBehaviour.OnMove += HandleCardDrag;
        BaseCardBehaviour.OnMoveRelease += PerformRelease;
    }
    protected override void OnDisable() {
        base.OnDisable();

        BaseCardBehaviour.OnHoverEnter -= SetCardsToMoveOver;
        BaseCardBehaviour.OnHoverExit -= SetCardsBackToStandardPos;
        BaseCardBehaviour.OnMove -= HandleCardDrag;
        BaseCardBehaviour.OnMoveRelease -= PerformRelease;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.V))
            GiveCard();
    }

    protected override void AddCard(AbilityCard card) {
        base.AddCard(card);

        CardAssetHolder cardObject = cards[^1];
        cardObject.Name.text = card.Name;
        cardObject.Discription.text = card.Discription;
        cardObject.Icon.sprite = card.Icon;
        cardObject.ManaCost.text = card.ManaCost.ToString();
    }

    protected override void LineOutCards() {
        int numObjects = cards.Count;
        float arcAngle = Mathf.Min((numObjects - 1) * spacing, maxWidth);
        float angleIncrement = arcAngle == 0 ? 0 : arcAngle / (numObjects - 1);
        float startAngle = -arcAngle / 2f;

        for (int i = 0; i < cards.Count; i++) {
            CardAssetHolder card = cards[i];

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
                new DoMethodAction(() => card.cardBehaviour?.SetValues(position + new Vector3(0, -radius, 0) + new Vector3(0, raisedAmount, 0), uiCam, index))
            });
        }
    }

    private void SetCardsToMoveOver(BaseCardBehaviour raisedCard, System.Action actionForRaisedCard) {
        if (cards.Where(x => x.cardBehaviour.CanInvoke == false).ToList().Count > 0)
            return;

        bool hasReachedraisedCard = false;
        foreach (CardAssetHolder card in cards) {
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
        if (cards.Where(x => x.cardBehaviour.CanInvoke == false).ToList().Count > 0)
            return;

        foreach (CardAssetHolder card in cards) {
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

    private void HandleCardDrag(BaseCardBehaviour card) {
        hasCardFaded = false;
        CanvasGroup canvasGroup = cards[card.Index].Fader;

        float targetAlpha = card.transform.position.y > transform.position.y + fadeThreshold ? 0.0f : 1.0f;
        float scaler = Mathf.Clamp(Mathf.Abs((transform.position.y + fadeThreshold) - card.transform.position.y) * .5f, 0, 1);
        float newAlpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, scaler);
        canvasGroup.alpha = newAlpha;

        hasCardFaded = newAlpha < .1f;

        if (hasCardFaded != hasCardFadedCallRan) {
            if (hasCardFaded) {
                GridStaticFunctions.ResetTileColors();

                GridStaticFunctions.HighlightTiles(GridStaticSelectors.GetPositions(
                    abilityCards[card.Index].availabletilesSelector,
                    GridStaticFunctions.CONST_EMPTY),
                    HighlightType.MovementHighlight);
            }
            else
                GridStaticFunctions.ResetTileColors();

            EventManager<CameraEventType, Selector>.Invoke(CameraEventType.CHANGE_CAM_SELECTOR, hasCardFaded ? abilityCards[card.Index].areaOfEffectSelector : null);
            hasCardFadedCallRan = hasCardFaded;
        }
    }

    private void PerformRelease(BaseCardBehaviour card, System.Action actionForRaisedCard) {
        CanvasGroup canvasGroup = cards[card.Index].Fader;
        AbilityCard ability = abilityCards[card.Index];

        EventManager<CameraEventType, Selector>.Invoke(CameraEventType.CHANGE_CAM_SELECTOR, null);

        if (hasCardFaded) {
            List<Vector2Int> validTiles = GridStaticSelectors.GetPositions(ability.availabletilesSelector, MouseToWorldView.HoverTileGridPos);
            if (validTiles.Contains(MouseToWorldView.HoverTileGridPos)) {
                List<Vector2Int> affectedTiles = GridStaticSelectors.GetPositions(ability.areaOfEffectSelector, MouseToWorldView.HoverTileGridPos);

                GridStaticFunctions.ResetTileColors();
                AbilityManager.PerformAbility(ability, affectedTiles.ToArray());
                RemoveCard(card.Index);
                return;
            }
        }

        // If no other thing was done
        canvasGroup.alpha = 1;
        actionForRaisedCard.Invoke();
    }
}
