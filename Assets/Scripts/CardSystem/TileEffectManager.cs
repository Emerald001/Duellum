using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileEffectManager : MonoBehaviour {
    [SerializeField] private GameObject pickupCardPrefab;
    [SerializeField] private int cardsToSpawn;

    private Dictionary<Vector2Int, GameObject> visualsPerTile = new();

    private void OnEnable() {
        EventManager<BattleEvents, EventMessage<UnitController, Vector2Int>>.Subscribe(BattleEvents.UnitTouchedTileEffect, RunTileEffect);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, CleanUpEffects);
    }
    private void OnDisable() {
        EventManager<BattleEvents, EventMessage<UnitController, Vector2Int>>.Unsubscribe(BattleEvents.UnitTouchedTileEffect, RunTileEffect);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, CleanUpEffects);
    }

    public void SetUp() {
        for (int i = 0; i < cardsToSpawn; i++)
            SpawnCard();
    }

    private void RunTileEffect(EventMessage<UnitController, Vector2Int> message) {
        var effect = GridStaticFunctions.TileEffectPositions[message.value2];

        switch (effect) {
            case TileEffect.Card:
                Destroy(visualsPerTile[message.value2]);
                GridStaticFunctions.TileEffectPositions.Remove(message.value2);

                EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_PickUpCard");
                EventManager<UIEvents, int>.Invoke(UIEvents.GiveCard, message.value1.OwnerID);
                EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation,
                    $"Your <color=Blue>{message.value1.UnitBaseData.Name}</color> picked up a card");

                SpawnCard();
                break;
            case TileEffect.OnFire:
                break;
            case TileEffect.Trapped:
                break;
        }
    }

    private void SpawnCard() {
        List<Vector2Int> openGridPositions = GridStaticFunctions.GetAllOpenGridPositions();
        Vector2Int position = openGridPositions[Random.Range(0, openGridPositions.Count)];

        Vector3 worldPosition = GridStaticFunctions.CalcWorldPos(position);
        worldPosition.y += 0.5f;

        GameObject card = Instantiate(pickupCardPrefab, worldPosition, Quaternion.identity);
        visualsPerTile.Add(position, card);
        GridStaticFunctions.TileEffectPositions.Add(position, TileEffect.Card);
    }

    private void CleanUpEffects() {
        for (int i = GridStaticFunctions.TileEffectPositions.Count - 1; i >= 0; i--) {
            var tile = GridStaticFunctions.TileEffectPositions.Keys.ToList()[i];

            Destroy(visualsPerTile[tile]);
            GridStaticFunctions.TileEffectPositions.Remove(tile);
        }
    }
}
