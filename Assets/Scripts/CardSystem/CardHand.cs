using System.Collections.Generic;
using UnityEngine;

public abstract class CardHand : MonoBehaviour {
    [Header("References")]
    [SerializeField] protected Camera uiCam;
    [SerializeField] private CardAssetHolder cardPrefab;
    [SerializeField] protected CardStack cardStack;
    [SerializeField] private Transform stackPos;

    [Header("Card Move Values")]
    [SerializeField] protected float cardSpawnMoveSpeed;
    [SerializeField] protected float cardRotationSpeed;

    [Header("Card Spacing Values")]
    [SerializeField] protected float spacing;
    [SerializeField] protected float maxWidth;
    [SerializeField] protected float radius;
    [SerializeField] protected float raisedAmount;

    public int OwnerID { get; set; }
    public List<AbilityCard> AbilityCards => abilityCards;

    protected readonly List<CardAssetHolder> cards = new();
    protected readonly List<AbilityCard> abilityCards = new();

    protected virtual void OnEnable() {
        EventManager<UIEvents, int>.Subscribe(UIEvents.GiveCard, GiveCard);
        EventManager<UIEvents, EventMessage<int, AbilityCard>>.Subscribe(UIEvents.GiveCard, GiveSpecificCard);
    }
    protected virtual void OnDisable() {
        EventManager<UIEvents, int>.Unsubscribe(UIEvents.GiveCard, GiveCard);
        EventManager<UIEvents, EventMessage<int, AbilityCard>>.Unsubscribe(UIEvents.GiveCard, GiveSpecificCard);
    }

    private void Start() {
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

        cards.Add(cardObject);
        abilityCards.Add(card);
        LineOutCards();
    }

    protected virtual void RemoveCard(int index) {
        CardAssetHolder card = cards[index];

        cards.RemoveAt(index);
        abilityCards.RemoveAt(index);

        Destroy(card.gameObject);

        LineOutCards();
    }

    public void RemoveSpecificCard(AbilityCard card) {
        if (!abilityCards.Contains(card))
            return;

        int index = abilityCards.IndexOf(card);
        RemoveCard(index);
    }

    protected abstract void LineOutCards();
}
