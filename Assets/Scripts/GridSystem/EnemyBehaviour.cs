using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBehaviour : MonoBehaviour {
    [SerializeField] private Transform visualsParent;
    [SerializeField] private float viewRange;
    [SerializeField] private float rotationSpeed;

    public List<UnitData> EnemyTeam => team;
    private List<UnitData> team;

    public Vector2Int GridPosition => gridPos;
    private Vector2Int gridPos;

    private Transform player;
    private Animator unitAnimator;

    private bool hasFound = false;

    public void Setup(Transform player, List<UnitData> enemyTeam, Vector2Int gridPos) {
        this.player = player;
        this.team = enemyTeam;
        this.gridPos = gridPos;

        var unitVisual = team[Random.Range(0, team.Count)].PawnPrefab;
        var unit = Instantiate(unitVisual, visualsParent);

        unitAnimator = GetComponentInChildren<Animator>();
    }

    private void Update() {
        if (hasFound)
            return;

        Vector3 direction = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position + new Vector3(0, .2f, 0), direction, out var hit, viewRange)) {
            if (hit.collider.CompareTag("Player")) {
                hasFound = true;

                EventManager<BattleEvents, EnemyBehaviour>.Invoke(BattleEvents.EnemyViewedPlayer, this);
                StartCoroutine(ViewSequence());
            }
        }
    }

    private IEnumerator ViewSequence() {
        Quaternion lookDir = Quaternion.LookRotation(player.transform.position - transform.position, Vector3.up);

        unitAnimator.SetBool("Walking", true);
        while (Vector3.Distance(transform.eulerAngles, lookDir.eulerAngles) > .1f) {
            transform.rotation = Quaternion.Lerp(transform.rotation, lookDir, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        unitAnimator.SetBool("Walking", false);

        yield return new WaitForSeconds(1f);

        unitAnimator.SetTrigger("Attacking");

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}