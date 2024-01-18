using UnityEngine;
using System.Collections.Generic;

public class DisplayUnitCards : MonoBehaviour {
    [SerializeField] private GameObject cardHolder;
    [SerializeField] private CharacterCard unitCard;

    [SerializeField] private Transform playerHolder;
    [SerializeField] private Transform enemyHolder;

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartBattle, DisplayEnemyCards);
        EventManager<DungeonEvents>.Subscribe(DungeonEvents.GenerationDone, DisplayPlayerCards);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, RemoveEnemyCards);
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartBattle, DisplayEnemyCards);
        EventManager<DungeonEvents>.Unsubscribe(DungeonEvents.GenerationDone, DisplayPlayerCards);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, RemoveEnemyCards);
    }

    private void DisplayPlayerCards() {
        List<UnitData> playerTeam = UnitStaticManager.PlayerPickedUnits;

        float totalWidth = 3 * (playerTeam.Count + 1);
        float dis = totalWidth / (playerTeam.Count + 1);

        for (int i = 0; i < playerTeam.Count; i++) {
            GameObject holder = Instantiate(cardHolder, playerHolder);
            holder.transform.position = new Vector3(playerHolder.position.x + dis * (i + 1) - totalWidth / 2, playerHolder.position.y, playerHolder.position.z);

            CharacterCard card = Instantiate(unitCard, playerHolder);
            card.transform.position = new Vector3(playerHolder.position.x + dis * (i + 1) - totalWidth / 2, playerHolder.position.y + .1f, playerHolder.position.z);
            card.SetUp(playerTeam[i], new Vector3(playerHolder.position.x + dis * (i + 1) - totalWidth / 2, playerHolder.position.y + 2.6f, playerHolder.position.z));
        }
    }

    private void DisplayEnemyCards(BattleData data) {
        List<UnitData> enemyTeam = data.EnemyUnits;

        float totalWidth = 3 * (enemyTeam.Count + 1);
        float dis = totalWidth / (enemyTeam.Count + 1);

        for (int i = 0; i < enemyTeam.Count; i++) {
            GameObject holder = Instantiate(cardHolder, enemyHolder);
            holder.transform.position = new Vector3(enemyHolder.position.x + dis * (i + 1) - totalWidth / 2, enemyHolder.position.y, enemyHolder.position.z);
            holder.transform.rotation = Quaternion.Euler(0, 0, 180);

            CharacterCard card = Instantiate(unitCard, enemyHolder);
            card.transform.position = new Vector3(enemyHolder.position.x + dis * (i + 1) - totalWidth / 2, enemyHolder.position.y - .1f, enemyHolder.position.z);
            card.SetUp(enemyTeam[i], new Vector3(enemyHolder.position.x + dis * (i + 1) - totalWidth / 2, enemyHolder.position.y - 2.6f, enemyHolder.position.z));
        }
    }

    private void RemoveEnemyCards() {
        for (int i = enemyHolder.childCount - 1; i >= 0; i--) {
            Destroy(enemyHolder.GetChild(0).gameObject);
        }
    }
}
