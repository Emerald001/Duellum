using System.Collections.Generic;
using Unity.VisualScripting;
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
        AbilityCard card;
        // Check if the typeName is a shortcut name in the dictionary
        if (cardShortcuts.TryGetValue(name, out card)) {
            Debug.Log(name, card);
            currentCards.Remove(card);
            return card;
        }
        else {
            // Handle the case where the typeName doesn't match any shortcut name
            return null;
        }
    }
}
