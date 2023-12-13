using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridCardManager : MonoBehaviour {
    [SerializeField] private GameObject pickupCardPrefab;
    [SerializeField] private int cardsToSpawn;

    private void OnEnable() {
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.PickUpAbilityCard, PickUpCard);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, CleanUpCards);
    }
    private void OnDisable() {
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.PickUpAbilityCard, PickUpCard);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, CleanUpCards);
    }

    public void SetUp() {
        for (int i = 0; i < cardsToSpawn; i++)
            SpawnCard();
    }

    private void SpawnCard() {
        List<Vector2Int> openGridPositions = GridStaticFunctions.GetAllOpenGridPositions();
        Vector2Int position = openGridPositions[Random.Range(0, openGridPositions.Count)];

        Vector3 worldPosition = GridStaticFunctions.CalcWorldPos(position);
        worldPosition.y += 0.5f;

        GameObject card = Instantiate(pickupCardPrefab, worldPosition, Quaternion.identity);
        GridStaticFunctions.CardPositions.Add(position, card);
    }

    private void PickUpCard(UnitController unit) {
        if (UnitStaticManager.PlayerUnitsInPlay.Contains(unit)) {
            EventManager<UIEvents>.Invoke(UIEvents.GivePlayerCard);
            EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, 
                $"Your <color=green>{unit.UnitBaseData.Name}</color> picked up a card");
        }
        else {
            EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, 
                $"Enemy <color=red>{unit.UnitBaseData.Name}</color> picked up a card");
            EventManager<UIEvents>.Invoke(UIEvents.GiveEnemyCard);
        }

        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_PickUpCard");
        SpawnCard();
    }

    private void CleanUpCards() {
        for (int i = GridStaticFunctions.CardPositions.Count - 1; i >= 0; i--) {
            var tile = GridStaticFunctions.CardPositions.Keys.ToList()[i];

            Destroy(GridStaticFunctions.CardPositions[tile]);
            GridStaticFunctions.CardPositions.Remove(tile);
        }
    }
}
