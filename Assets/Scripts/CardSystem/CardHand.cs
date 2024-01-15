using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CardHand : MonoBehaviour {
    public static System.Action<Vector2Int> OnPickPosition;
    public static System.Action OnUndo;

    [Header("References")]
    [SerializeField] protected Camera uiCam;
    [SerializeField] private CardAssetHolder cardPrefab;
    [SerializeField] protected CardStack cardStack;
    [SerializeField] private Transform stackPos;
    [SerializeField] protected Transform selectedPosition;

    [Header("Card Move Values")]
    [SerializeField] protected float cardSpawnMoveSpeed;
    [SerializeField] protected float cardRotationSpeed;

    [Header("Card Spacing Values")]
    [SerializeField] protected float spacing;
    [SerializeField] protected float maxWidth;
    [SerializeField] protected float radius;
    [SerializeField] protected float raisedAmount;

    public int OwnerID { get; private set; }
    public List<Card> Cards => cards;

    protected List<CardAssetHolder> cardVisuals = new();
    protected List<Card> cards = new();

    protected CardHandStateMachine CardHandStateMachine;

    protected virtual void OnEnable() {
        EventManager<UIEvents, int>.Subscribe(UIEvents.GiveCard, GiveCard);
        EventManager<UIEvents, EventMessage<int, AbilityCard>>.Subscribe(UIEvents.GiveCard, GiveSpecificCard);
    }
    protected virtual void OnDisable() {
        EventManager<UIEvents, int>.Unsubscribe(UIEvents.GiveCard, GiveCard);
        EventManager<UIEvents, EventMessage<int, AbilityCard>>.Unsubscribe(UIEvents.GiveCard, GiveSpecificCard);
    }

    public void SetHand(int ID) {
        OwnerID = ID;
        CardHandStateMachine = new(OwnerID);
        cardStack.ResetDeck();
    }

    protected void GiveCard(int id) {
        if (id != OwnerID)
            return;

        AbilityCard card = cardStack.GetCard();
        if (card != null)
            AddCard(card);
    }

    private void GiveSpecificCard(EventMessage<int, AbilityCard> message) {
        if (message.value1 != OwnerID)
            return;

        if (message.value2 != null)
            AddCard(message.value2);
    }

    protected virtual void AddCard(AbilityCard card) {
        CardAssetHolder cardObject = Instantiate(cardPrefab, stackPos.position, stackPos.rotation);

        cardVisuals.Add(cardObject);
        cards.Add(card);

        LineOutCards();
    }

    protected virtual void RemoveCard(int index) {
        CardAssetHolder card = cardVisuals[index];

        cardVisuals.RemoveAt(index);
        cards.RemoveAt(index);

        Destroy(card.gameObject);
        LineOutCards();
    }

    public void RemoveSpecificCard(Card card) {
        if (!cards.Contains(card))
            return;

        int index = cards.IndexOf(card);
        RemoveCard(index);
    }

    protected void SortCards() {
        var tmp = cards.Zip(cardVisuals, (k, v) => new { k, v })
           .OrderByDescending(x => (int)x.k.CardType)
           .ToList();
        tmp.Reverse();

        cards = tmp.Select(x => x.k).ToList();
        cardVisuals = tmp.Select(x => x.v).ToList();
    }

    protected abstract void LineOutCards();
}