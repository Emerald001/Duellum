using UnityEngine;

public class BattleEvaluator : MonoBehaviour {
    [SerializeField] private PlayerController player;

    private void OnEnable() {
        EventManager<BattleEvents, EnemyBehaviour>.Subscribe(BattleEvents.EnemyViewedPlayer, OnEnemyViewPlayer);
    }
    private void OnDisable() {
        EventManager<BattleEvents, EnemyBehaviour>.Unsubscribe(BattleEvents.EnemyViewedPlayer, OnEnemyViewPlayer);
    }

    private void OnEnemyViewPlayer(EnemyBehaviour behaviour) {
        BattleData data = new(player.PlayerPosition, behaviour.GridPosition, UnitStaticManager.PlayerPickedUnits, behaviour.EnemyTeam);
        EventManager<BattleEvents, BattleData>.Invoke(BattleEvents.StartBattle, data);
    }
}
