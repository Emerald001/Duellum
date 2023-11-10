using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera uiCam;
    [SerializeField] private CardAssetHolder cardPrefab;
    [SerializeField] private CardStack cardStack;
    [SerializeField] private Transform stackPos;

    [Header("Card Move Values")]
    [SerializeField] private float cardSpawnMoveSpeed;
    [SerializeField] private float cardViewMoveSpeed;
    [SerializeField] private float cardRotationSpeed;

    [Header("Card Spacing Values")]
    [SerializeField] private float spacing;
    [SerializeField] private float maxWidth;
    [SerializeField] private float radius;
    [SerializeField] private float moveOverDistance;
    [SerializeField] private float raisedAmount;
    [SerializeField] private float fadeThreshold;

    private readonly List<CardAssetHolder> cards = new();
    private readonly List<AbilityCard> abilityCards = new();

    private bool hasCardFaded;
    private bool hasCardFadedCallRan;

    private void OnEnable() {
        CardBehaviour.OnHoverEnter += SetCardsToMoveOver;
        CardBehaviour.OnHoverExit += SetCardsBackToStandardPos;
        CardBehaviour.OnMove += HandleCardDrag;
        CardBehaviour.OnMoveRelease += PerformRelease;
    }

    private void Start() {
        cardStack.ResetDeck();

        AddCard(cardStack.GetCard());
        AddCard(cardStack.GetCard());
        AddCard(cardStack.GetCard());
        //nextCard = Instantiate(cardPrefab, stackPos.position, stackPos.rotation);
        //nextCard.transform.parent = stackPos;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.V)) {
            AbilityCard card = cardStack.GetCard();
            if (card != null) 
                AddCard(card);
        }
    }

    public void AddCard(AbilityCard card) {
        CardAssetHolder cardObject = Instantiate(cardPrefab, stackPos.position, stackPos.rotation);

        cardObject.Name.text = card.Name;
        cardObject.Discription.text = card.Discription;
        cardObject.Icon.sprite = card.Icon;
        //cardObject.Background.sprite = card.Background;
        cardObject.ManaCost.text = card.ManaCost.ToString();

        cards.Add(cardObject);
        abilityCards.Add(card);
        LineOutCards();
    }

    public void RemoveCard(int index) {
        CardAssetHolder card = cards[index];

        cards.RemoveAt(index);
        abilityCards.RemoveAt(index);

        Destroy(card.gameObject);

        LineOutCards();
    }

    private void LineOutCards() {
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

            Vector3 position = transform.position + new Vector3(x, y,  i * .01f);
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, position - transform.position);

            int index = i;
            card.cardBehaviour.ClearQueue();
            card.cardBehaviour.SetActionQueue(new List<Action>() {
                new DoMethodAction(() => card.cardBehaviour.CanInvoke = false),
                new ActionStack(
                    new MoveObjectAction(card.gameObject, cardSpawnMoveSpeed, position + new Vector3(0, -radius, 0)),
                    new RotateAction(card.gameObject, rotation.eulerAngles, cardRotationSpeed, .01f)
                ),
                new DoMethodAction(() => card.cardBehaviour.SetValues(position + new Vector3(0, -radius, 0) + new Vector3(0, raisedAmount, 0), uiCam, index))
            });
        }
    }

    private void SetCardsToMoveOver(CardBehaviour raisedCard, System.Action actionForRaisedCard) {
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

    private void SetCardsBackToStandardPos(CardBehaviour raisedCard, System.Action actionForRaisedCard) {
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

    private void HandleCardDrag(CardBehaviour card) {
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

    private void PerformRelease(CardBehaviour card, System.Action actionForRaisedCard) {
        CanvasGroup canvasGroup = cards[card.Index].Fader;
        var ability = abilityCards[card.Index];

        EventManager<CameraEventType, Selector>.Invoke(CameraEventType.CHANGE_CAM_SELECTOR, null);

        if (hasCardFaded) {
            List<Vector2Int> validTiles = GridStaticSelectors.GetPositions(ability.availabletilesSelector, MouseToWorldView.HoverTileGridPos);
            if (validTiles.Contains(MouseToWorldView.HoverTileGridPos)) {
                var affectedTiles = GridStaticSelectors.GetPositions(ability.areaOfEffectSelector, MouseToWorldView.HoverTileGridPos);

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
