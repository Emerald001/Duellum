using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class CardPickerManager : MonoBehaviour {
    [SerializeField] private UICard cardPrefab;
    [SerializeField] private Transform cardTransform;
    [SerializeField] private Transform pickedcardTransform;
    [SerializeField] private List<UnitData> data = new();

    private List<UICard> pickedCardsUI = new(3);
    private List<UnitData> pickedCards = new(3);

    private UnitData slot1 = null;
    private UnitData slot2 = null;
    private UnitData slot3 = null;

    private void Awake() {
        foreach (var item in data) {
            UICard card = Instantiate(cardPrefab, cardTransform);

            UnitData data = item;
            card.SetCardData(data);
            card.GetComponent<Button>().onClick.AddListener(() => PickCard(data));
        }

        for (int i = 0; i < 3; i++) {
            UICard card = Instantiate(cardPrefab, pickedcardTransform);
            pickedCardsUI.Add(card);
        }
    }

    public void PickCard(UnitData data) {
        Debug.Log(1);

        if (pickedCards.Contains(data)) {
            pickedCards.Remove(data);

            if (slot1 == data)
                slot1 = null;
            else if (slot2 == data)
                slot2 = null;
            else if (slot3 == data)
                slot3 = null;
        }
        else {
            pickedCards.Add(data);

            if (slot1 == null) 
                slot1 = data;
            else if (slot2 == null)
                slot2 = data;
            else if (slot3 == null)
                slot3 = data;
        }

        DrawCards();
    }

    private void DrawCards() {
        if (slot1 != null) {
            pickedCardsUI[0].SetCardData(slot1);
            pickedCardsUI[0].GetComponent<Button>().onClick.AddListener(() => PickCard(slot1));
        }
        else {
            pickedCardsUI[0].SetCardData(null);
            pickedCardsUI[0].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (slot2 != null) {
            pickedCardsUI[1].SetCardData(slot2);
            pickedCardsUI[1].GetComponent<Button>().onClick.AddListener(() => PickCard(slot2));
        }
        else {
            pickedCardsUI[1].SetCardData(null);
            pickedCardsUI[1].GetComponent<Button>().onClick.RemoveAllListeners();
        }

        if (slot3 != null) {
            pickedCardsUI[2].SetCardData(slot3);
            pickedCardsUI[2].GetComponent<Button>().onClick.AddListener(() => PickCard(slot3));
        }
        else {
            pickedCardsUI[2].SetCardData(null);
            pickedCardsUI[2].GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }

    public void Btn_ConfirmPick() {
        if (pickedCards.Count < 1)
            return;

        gameObject.SetActive(false);
        EventManager<DungeonEvents>.Invoke(DungeonEvents.StartGeneration);
    }
}
