using System.Collections.Generic;
using UnityEngine;

public class UnitAttackModule {
    public List<Vector2Int> AttackableTiles => currentAttackableTiles;

    private readonly List<Vector2Int> currentAttackableTiles = new();
    private readonly UnitAttack attack;

    public UnitAttackModule(UnitAttack attack) {
        this.attack = attack;
    }

    public void FindAttackableTiles(List<Vector2Int> gridpositions, List<UnitController> enemies) {
        currentAttackableTiles.Clear();

        List<Vector2Int> tiles = new();
        for (int i = 0; i < gridpositions.Count; i++)
            tiles.AddRange(GridStaticSelectors.GetPositions(attack.ApplicableTilesSelector, gridpositions[i]));

        foreach (var tile in tiles) {
            if (UnitStaticManager.TryGetUnitFromGridPos(tile, out var unit))
                if (enemies.Contains(unit))
                    currentAttackableTiles.Add(tile);
        }
    }

    public Vector2Int GetClosestTile(Vector2Int tileToAttackPosition, Vector2Int attackingUnitPos, Vector3 worldpoint, List<Vector2Int> accessableTiles) {
        float smallestDistance = Mathf.Infinity;
        Vector2Int closestTile = GridStaticFunctions.CONST_EMPTY;

        GridStaticFunctions.RippleThroughSquareGridPositions(tileToAttackPosition, attack.ApplicableTilesSelector.range + 1, (neighbour, j) => {
            if (!accessableTiles.Contains(neighbour) && neighbour != attackingUnitPos)
                return;

            if (Vector3.Distance(GridStaticFunctions.CalcSquareWorldPos(neighbour), worldpoint) < smallestDistance) {
                smallestDistance = Vector3.Distance(GridStaticFunctions.CalcSquareWorldPos(neighbour), worldpoint);
                closestTile = neighbour;
            }
        });

        return closestTile;
    }
}
