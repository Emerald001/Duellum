using System.Collections.Generic;
using UnityEngine;

public class UnitAttackModule {
    public List<Vector2Int> AttackableTiles => currentAttackableTiles;
    public int OwnerID { get; private set; }

    private readonly List<Vector2Int> currentAttackableTiles = new();
    private readonly Dictionary<Vector2Int, List<Vector2Int>> currentAttackableTilesWithStandingPosition = new();
    private readonly UnitAttack attack;

    public UnitAttackModule(UnitAttack attack, int id) {
        OwnerID = id;
        this.attack = attack;
    }

    public void FindAttackableTiles(List<Vector2Int> gridpositions, List<UnitController> enemies) {
        currentAttackableTilesWithStandingPosition.Clear();
        currentAttackableTiles.Clear();

        for (int i = 0; i < gridpositions.Count; i++) {
            List<Vector2Int> tiles = GridStaticSelectors.GetPositions(attack.ApplicableTilesSelector, gridpositions[i], OwnerID);
            currentAttackableTilesWithStandingPosition.Add(gridpositions[i], new());

            foreach (var tile in tiles) {
                if (UnitStaticManager.TryGetUnitFromGridPos(tile, out var unit) && enemies.Contains(unit)) {
                    currentAttackableTilesWithStandingPosition[gridpositions[i]].Add(tile);
                    currentAttackableTiles.Add(tile);
                }
            }
        }
    }

    public Vector2Int GetClosestTile(Vector2Int tileToAttackPosition) {
        List<Vector2Int> pickableTiles = GetAvailablePositions(tileToAttackPosition);

        return attack.isRanged
            ? GetClosestPointRanged(tileToAttackPosition, pickableTiles)
            : GetClosestPointMelee(tileToAttackPosition, pickableTiles);
    }

    //private Vector2Int GetClosestTile(Vector3 target, List<Vector2Int> availableTiles) {
    //    float smallestDistance = Mathf.Infinity;
    //    Vector2Int closestTile = GridStaticFunctions.CONST_EMPTY;

    //    foreach (var tile in availableTiles) {
    //        if (Vector3.Distance(GridStaticFunctions.CalcWorldPos(tile), target) < smallestDistance) {
    //            smallestDistance = Vector3.Distance(GridStaticFunctions.CalcWorldPos(tile), target);
    //            closestTile = tile;
    //        }
    //    }

    //    return closestTile;
    //}

    private Vector2Int GetClosestPointMelee(Vector2Int targetPosition, List<Vector2Int> availableTiles) {
        if (!UnitStaticManager.TryGetUnitFromGridPos(targetPosition, out var enemy))
            return targetPosition;

        Vector2Int lookDir = enemy.LookDirection;
        Vector2Int sideDir = new(enemy.LookDirection.y, enemy.LookDirection.x);

        List<Vector2Int> attackPositions = new() {
            targetPosition - lookDir,
            targetPosition - sideDir,
            targetPosition + sideDir,
            targetPosition + lookDir,
        };

        foreach (var item in attackPositions) {
            if (availableTiles.Contains(item))
                return item;
        }

        throw new System.Exception("Something with targeting went very wrong!");
    }

    private Vector2Int GetClosestPointRanged(Vector2Int targetPosition, List<Vector2Int> availableTiles) {
        if (!UnitStaticManager.TryGetUnitFromGridPos(targetPosition, out var enemy))
            return targetPosition;

        Vector2Int lookDir = enemy.LookDirection;
        Vector2Int sideDir = new(enemy.LookDirection.y, enemy.LookDirection.x);

        List<Vector2Int> directions = new() {
            -lookDir,
            -sideDir,
            sideDir,
            lookDir,
        };

        foreach (var item in directions) {
            var list = GridStaticFunctions.GetLine(targetPosition, item, attack.ApplicableTilesSelector.range);
            list.Reverse();

            foreach (var position in list) {
                if (availableTiles.Contains(position))
                    return position;
            }
        }

        throw new System.Exception("Something with targeting went very wrong!");
    }

    private List<Vector2Int> GetAvailablePositions(Vector2Int attackablePos) {
        List<Vector2Int> result = new();

        foreach (var item in currentAttackableTilesWithStandingPosition) {
            if (item.Value.Contains(attackablePos))
                result.Add(item.Key);
        }

        return result;
    }
}
