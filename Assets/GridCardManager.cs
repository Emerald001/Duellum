using UnityEngine;

public class GridCardManager : MonoBehaviour {
    [SerializeField] private int cardsToSpawn;
    [SerializeField] private GameObject pickupCardPrefab;
    
    public void SetUp() {
        for (int i = 0; i < cardsToSpawn; i++) {
            SpawnCard();
        }
    }

    public void SpawnCard() {
        var tmp = GridStaticFunctions.GetAllOpenGridPositions();

        int randomSpot = Random.Range(0, tmp.Count);
        
    }

    public void PickUpCard() {


        SpawnCard();
    }
}
