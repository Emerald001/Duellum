using System.Collections.Generic;
using UnityEngine;

public class UnitAttackModule {
    public List<Vector2Int> AttackableTiles => currentAttackableTiles;

    private readonly List<Vector2Int> currentAttackableTiles = new();
    private readonly Dictionary<Vector2Int, List<Vector2Int>> currentAttackableTilesWithStandingPosition = new();
    private readonly UnitAttack attack;

    public UnitAttackModule(UnitAttack attack) {
        this.attack = attack;
    }

    public void FindAttackableTiles(List<Vector2Int> gridpositions, List<UnitController> enemies) {
        currentAttackableTilesWithStandingPosition.Clear();
        currentAttackableTiles.Clear();

        for (int i = 0; i < gridpositions.Count; i++) {
            List<Vector2Int> tiles = GridStaticSelectors.GetPositions(attack.ApplicableTilesSelector, gridpositions[i]);
            currentAttackableTilesWithStandingPosition.Add(gridpositions[i], new());

            foreach (var tile in tiles) {
                if (UnitStaticManager.TryGetUnitFromGridPos(tile, out var unit) && enemies.Contains(unit)) {
                    currentAttackableTilesWithStandingPosition[gridpositions[i]].Add(tile);
                    currentAttackableTiles.Add(tile);
                }
            }
        }
    }

    public Vector2Int GetClosestTile(Vector2Int tileToAttackPosition, Vector2Int attackingUnitPos, Vector3 worldpoint) {
        List<Vector2Int> pickableTiles = GetAvailablePositions(tileToAttackPosition);

        return attack.isRanged
            ? GetClosestTile(GridStaticFunctions.Grid[attackingUnitPos].transform.position, pickableTiles)
            : GetClosestTile(worldpoint, pickableTiles);
    }

    private Vector2Int GetClosestTile(Vector3 target, List<Vector2Int> availableTiles) {
        float smallestDistance = Mathf.Infinity;
        Vector2Int closestTile = GridStaticFunctions.CONST_EMPTY;

        foreach (var tile in availableTiles) {
            if (Vector3.Distance(GridStaticFunctions.CalcSquareWorldPos(tile), target) < smallestDistance) {
                smallestDistance = Vector3.Distance(GridStaticFunctions.CalcSquareWorldPos(tile), target);
                closestTile = tile;
            }
        }

        return closestTile;
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
