using UnityEngine;

public class GridCardManager : MonoBehaviour {
    [SerializeField] private int cardsToSpawn;
    [SerializeField] private GameObject pickupCardPrefab;

    private void OnEnable() {
        EventManager<BattleEvents>.Subscribe(BattleEvents.SpawnAbilityCard, PickUpCard);
    }

    private void OnDisable() {
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.SpawnAbilityCard, PickUpCard);
    }
    public void SetUp() {
        for (int i = 0; i < cardsToSpawn; i++) {
            SpawnCard();
        }
    }

    public void SpawnCard() {
        var tmp = GridStaticFunctions.GetAllOpenGridPositions();

        int randomSpot = Random.Range(0, tmp.Count);
        Vector3 worldPosition = GridStaticFunctions.CalcSquareWorldPos(tmp[randomSpot]);
        worldPosition.y = worldPosition.y + 0.5f;
        
        Instantiate(pickupCardPrefab, worldPosition, Quaternion.identity);
        
    }

    public void PickUpCard() {
        EventManager<BattleEvents>.Invoke(BattleEvents.GiveAbilityCard);
        SpawnCard();
    }
}
