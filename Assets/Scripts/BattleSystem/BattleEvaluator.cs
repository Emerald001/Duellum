using System.Collections.Generic;
using UnityEngine;

public class BattleEvaluator : MonoBehaviour {
    [SerializeField] private PlayerController player;
    [SerializeField] private List<UnitData> playerTeam;

    private void OnEnable() {
        EventManager<BattleEvents, EnemyBehaviour>.Subscribe(BattleEvents.EnemyViewedPlayer, OnEnemyViewPlayer);
    }
    private void OnDisable() {
        EventManager<BattleEvents, EnemyBehaviour>.Unsubscribe(BattleEvents.EnemyViewedPlayer, OnEnemyViewPlayer);
    }

    private void OnEnemyViewPlayer(EnemyBehaviour behaviour) {
        BattleData data = new(player.PlayerPosition, behaviour.GridPosition, playerTeam, behaviour.EnemyTeam);

        Debug.Log($"Player position: {player.PlayerPosition}");
        Debug.Log($"Enemy position: {behaviour.GridPosition}");
        Debug.Log($"Player unit Amount: {playerTeam.Count}");
        Debug.Log($"Enemy unit Amount: {behaviour.EnemyTeam.Count}");

        EventManager<BattleEvents, BattleData>.Invoke(BattleEvents.StartBattle, data);
    }
}
