using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour {
    [SerializeField] private float viewRange;

    public List<UnitData> EnemyTeam => team;
    private List<UnitData> team;

    public Vector2Int GridPosition => gridPos;
    private Vector2Int gridPos;

    private Transform player;
    private bool Running;

    public void Setup(Transform player, List<UnitData> enemyTeam, Vector2Int gridPos) {
        this.player = player;
        this.team = enemyTeam;
        this.gridPos = gridPos;
    }

    private void Update() {
        Vector3 direction = (player.position - transform.position).normalized;

        if (Physics.Raycast(transform.position + new Vector3(0, .2f, 0), direction, out var hit, viewRange)) {
            if (hit.collider.CompareTag("Player")) {
                EventManager<BattleEvents, EnemyBehaviour>.Invoke(BattleEvents.EnemyViewedPlayer, this);

                Destroy(gameObject);
            }
        }
    }
}