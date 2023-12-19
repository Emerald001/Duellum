using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitController : UnitController {
    private float timer;
    private bool pickedAction;

    private Vector2Int bestPickedPosition = GridStaticFunctions.CONST_EMPTY;
    private AbilityCard cardToUse;

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

    public override void OnExit() {
        base.OnExit();
    }

    private void PickAction() {
        if (attackModule.AttackableTiles.Contains(bestPickedPosition))
            PickedTile(bestPickedPosition, attackModule.GetClosestTile(bestPickedPosition, gridPosition, Vector3.zero));
        else if (movementModule.AccessableTiles.Contains(bestPickedPosition))
            PickedTile(bestPickedPosition, Vector2Int.zero);

        pickedAction = true;
    }

    public int PickEvaluatedAction(List<AbilityCard> cards) {
        bestPickedPosition = GridStaticFunctions.CONST_EMPTY;
        int result = 0;

        FindTiles();

        if (attackModule.AttackableTiles.Count != 0) {
            UnitController lastEnemy = null;

            for (int i = 0; i < attackModule.AttackableTiles.Count; i++) {
                if (!UnitStaticManager.TryGetUnitFromGridPos(attackModule.AttackableTiles[i], out var unit))
                    continue;

                if (values.currentStats.Attack > unit.Values.currentStats.Defence)
                    if (unit.Values.currentStats.Defence > lastEnemy.Values.currentStats.Defence)
                        lastEnemy = unit;
            }

            if (lastEnemy == null) {
                for (int i = 0; i < cards.Count; i++) {
                    if (cards[i].abilityType != AbilityCardType.ApplyEffect)
                        continue;

                    if (cards[i].effectToApply.type != EffectType.AttackBoost)
                        continue;

                    for (int j = 0; j < attackModule.AttackableTiles.Count; j++) {
                        if (!UnitStaticManager.TryGetUnitFromGridPos(attackModule.AttackableTiles[j], out var unit))
                            continue;

                        lastEnemy = lastEnemy ? lastEnemy : unit;
                        if (values.currentStats.Attack + cards[i].effectToApply.sevarity > unit.Values.currentStats.Defence)
                            if (unit.Values.currentStats.Defence > lastEnemy.Values.currentStats.Defence) {
                                cardToUse = cards[i];
                                lastEnemy = unit;
                            }
                    }
                }

                if (lastEnemy != null) {
                    bestPickedPosition = UnitStaticManager.UnitPositions[lastEnemy];
                    result += 40;
                }
            }
            else {
                bestPickedPosition = UnitStaticManager.UnitPositions[lastEnemy];
                result += 60;
            }
        }

        if (movementModule.AccessableTiles.Count > 0) {
            if (bestPickedPosition != GridStaticFunctions.CONST_EMPTY) {
                List<Vector2Int> path = movementModule.GetPath(attackModule.GetClosestTile(bestPickedPosition, gridPosition, Vector3.zero));
                foreach (Vector2Int card in GridStaticFunctions.TileEffectPositions.Keys) {
                    if (path.Contains(card))
                        result += 10;
                }
            }
            else {
                UnitController lastEnemy = null;
                List<UnitController> enemies = UnitStaticManager.GetEnemies(OwnerID);

                for (int i = 0; i < enemies.Count; i++) {
                    var unit = enemies[i];

                    lastEnemy = lastEnemy ? lastEnemy : unit;
                    if (values.currentStats.Attack > unit.Values.currentStats.Defence)
                        if (unit.Values.currentStats.Defence > lastEnemy.Values.currentStats.Defence)
                            lastEnemy = unit;
                }

                Vector2Int enemyPos = UnitStaticManager.GetUnitPosition(lastEnemy);
                Vector2Int closestPos = GridStaticFunctions.CONST_EMPTY;

                float leastDistance = Mathf.Infinity;
                foreach (var item in movementModule.AccessableTiles) {
                    closestPos = closestPos == GridStaticFunctions.CONST_EMPTY ? closestPos : item;

                    // if the unit is already close to an enemy, lower the score

                    float dis = Vector3.Distance(GridStaticFunctions.CalcWorldPos(item), GridStaticFunctions.CalcWorldPos(enemyPos));
                    if (dis < leastDistance) {
                        leastDistance = dis;
                        closestPos = item;
                    }
                }

                if (closestPos != GridStaticFunctions.CONST_EMPTY) {
                    bestPickedPosition = closestPos;
                    result += 20;

                    List<Vector2Int> path = movementModule.GetPath(bestPickedPosition);
                    foreach (Vector2Int card in GridStaticFunctions.TileEffectPositions.Keys) {
                        if (path.Contains(card))
                            result += 10;
                    }
                }
            }
        }

        return result;
    }

    public float WaitTime() {
        return timer -= Time.deltaTime;
    }
}