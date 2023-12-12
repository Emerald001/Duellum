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

    public List<AbilityCard> AbilityCards => abilityCards;

    protected readonly List<CardAssetHolder> cards = new();
    protected readonly List<AbilityCard> abilityCards = new();

    protected virtual void OnEnable() {
        EventManager<BattleEvents>.Subscribe(BattleEvents.GiveAbilityCard, GiveCard);
    }
    protected virtual void OnDisable() {
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.GiveAbilityCard, GiveCard);
    }

    private void Start() {
        cardStack.ResetDeck();
    }

    protected void GiveCard() {
        AbilityCard card = cardStack.GetCard();
        if (card != null)
            AddCard(card);
    }

    private void GiveSpecificCard(string name) {
        AbilityCard card = cardStack.GetSpecificCard(name);
        if (card != null)
            AddCard(card);
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

    protected abstract void LineOutCards();
}
