using UnityEngine;

public class EnemyUnitInterface : UnitController {
    private float timer;
    private bool pickedAction;

    public override void OnEnter() {
        base.OnEnter();

        pickedAction = false;
        timer = .5f;
    }

    public override void OnUpdate() {
        base.OnUpdate();

        if (WaitTime() > 0)
            return;

        if (!pickedAction)
            PickAction();
    }

    private void PickAction() {
        if (attackModule.AttackableTiles.Count != 0) {
            UnitController lastEnemy = null;

            for (int i = 0; i < attackModule.AttackableTiles.Count; i++) {
                if (!UnitStaticManager.TryGetUnitFromGridPos(attackModule.AttackableTiles[i], out var unit))
                    continue;

                lastEnemy = lastEnemy ? lastEnemy : unit;
                if (unit.Values.currentStats.Defence < lastEnemy.Values.currentStats.Defence)
                    lastEnemy = unit;
            }

            Vector2Int pickedTile = UnitStaticManager.UnitPositions[lastEnemy];
            PickedTile(pickedTile, attackModule.GetClosestTile(pickedTile, gridPosition, Vector3.zero));
        }
        else if (movementModule.AccessableTiles.Count != 0) {
            Vector2Int pickedTile = GridStaticFunctions.CONST_EMPTY;

            for (int i = 0; i < movementModule.AccessableTiles.Count; i++) {
                if (movementModule.AccessableTiles[i].x < pickedTile.x)
                    pickedTile = movementModule.AccessableTiles[i];
            }

            PickedTile(pickedTile, Vector2Int.zero);
        }

        pickedAction = true;
    }

    public float WaitTime() {
        return timer -= Time.deltaTime;
    }
}