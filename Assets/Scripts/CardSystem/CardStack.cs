using System.Collections.Generic;
using UnityEngine;

public class CardStack : MonoBehaviour
{
    public List<AbilityCard> Cards;
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
}
