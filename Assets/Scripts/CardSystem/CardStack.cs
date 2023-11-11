using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour {
    [SerializeField] private List<AbilityCard> Cards;

    private readonly Dictionary<string, AbilityCard> cardShortcuts = new();
    private List<AbilityCard> currentCards;

    public void ResetDeck() {
        currentCards = new(Cards);
    }

    public AbilityCard GetCard() {
        int random = Random.Range(0, currentCards.Count);
        AbilityCard card = currentCards[random];

        currentCards.RemoveAt(random);
        return card;
    }

    public AbilityCard GetSpecificCard(string name) {
        if (cardShortcuts.TryGetValue(name, out AbilityCard card)) {
            Debug.Log(name, card);
            currentCards.Remove(card);
            return card;
        }
        else
            return null;
    }
}
