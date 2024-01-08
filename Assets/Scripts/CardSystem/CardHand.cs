using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public List<Card> Cards => cards;

    protected List<CardAssetHolder> cardVisuals = new();
    protected List<Card> cards = new();

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

        cardVisuals.Add(cardObject);
        cards.Add(card);

        LineOutCards();
    }

    protected virtual void RemoveCard(int index) {
        CardAssetHolder card = cardVisuals[index];

        cardVisuals.RemoveAt(index);
        cards.RemoveAt(index);

        Destroy(card.gameObject);
    }

    public void RemoveSpecificCard(AbilityCard card) {
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
