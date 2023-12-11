using System.Collections.Generic;
using UnityEngine;

public class GridCardManager : MonoBehaviour {
    [SerializeField] private GameObject pickupCardPrefab;
    [SerializeField] private int cardsToSpawn;

    private void OnEnable() {
        EventManager<BattleEvents>.Subscribe(BattleEvents.PickUpAbilityCard, PickUpCard);
    }
    private void OnDisable() {
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.PickUpAbilityCard, PickUpCard);
    }

    public void SetUp() {
        for (int i = 0; i < cardsToSpawn; i++)
            SpawnCard();
    }

    private void SpawnCard() {
        List<Vector2Int> openGridPositions = GridStaticFunctions.GetAllOpenGridPositions();
        Vector2Int position = openGridPositions[Random.Range(0, openGridPositions.Count)];

        Vector3 worldPosition = GridStaticFunctions.CalcSquareWorldPos(position);
        worldPosition.y += 0.5f;

        GameObject card = Instantiate(pickupCardPrefab, worldPosition, Quaternion.identity);
        GridStaticFunctions.CardPositions.Add(position, card);
    }

    private void PickUpCard() {
        EventManager<BattleEvents>.Invoke(BattleEvents.GiveAbilityCard);
        SpawnCard();
    }
}
