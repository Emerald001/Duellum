using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour {
    public List<AbilityCard> Cards;
    private List<AbilityCard> currentCards;

    private Dictionary<string, AbilityCard> cardShortcuts = new Dictionary<string, AbilityCard>();
    public void ResetDeck() {
        currentCards = new(Cards);

        //foreach (AbilityCard card in currentCards) {
        //    cardShortcuts.Add(card.shortcutName, card);
        //}
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
