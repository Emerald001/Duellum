using System.Collections.Generic;
using UnityEngine;

public class GridCardManager : MonoBehaviour {
    [SerializeField] private GameObject pickupCardPrefab;
    [SerializeField] private int cardsToSpawn;

    private void OnEnable() {
        EventManager<BattleEvents>.Subscribe(BattleEvents.SpawnAbilityCard, PickUpCard);
    }
    private void OnDisable() {
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.SpawnAbilityCard, PickUpCard);
    }

    public void SetUp() {
        for (int i = 0; i < cardsToSpawn; i++)
            SpawnCard();
    }

    private void SpawnCard() {
        List<Vector2Int> openGridPositions = GridStaticFunctions.GetAllOpenGridPositions();

        Vector3 worldPosition = GridStaticFunctions.CalcSquareWorldPos(openGridPositions[Random.Range(0, openGridPositions.Count)]);
        worldPosition.y += 0.5f;

        Instantiate(pickupCardPrefab, worldPosition, Quaternion.identity);
    }

    private void PickUpCard() {
        EventManager<BattleEvents>.Invoke(BattleEvents.GiveAbilityCard);
        SpawnCard();
    }
}
