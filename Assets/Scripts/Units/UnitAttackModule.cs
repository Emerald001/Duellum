using System.Collections.Generic;
using UnityEngine;

public class UnitAttackModule {
    public List<Vector2Int> AttackableTiles => currentAttackableTiles;

    private readonly List<Vector2Int> currentAttackableTiles = new();
    private readonly UnitAttack attack;

    public UnitAttackModule(UnitAttack attack) {
        this.attack = attack;
    }

    public void FindAttackableTiles(Vector2Int gridpos) {
        List<Vector2Int> tiles = GridStaticSelectors.GetPositions(attack.ApplicableTilesSelector, gridpos);

        List<Vector2Int> result = new();
        foreach (var tile in tiles) {
            if (UnitStaticManager.TryGetUnitFromGridPos(tile, out var unit))
                result.Add(tile);
        }
    }

    public Vector2Int GetClosestTile(Vector2Int gridPos, Vector2Int tile, Vector3 worldpoint, List<Vector2Int> accessableTiles) {
        float smallestDistance = Mathf.Infinity;
        
        Vector2Int closestTile = Vector2Int.zero;
        Vector2Int currentPos = tile;

        GridStaticFunctions.RippleThroughSquareGridPositions(gridPos, 2, (neighbour, j) => {
            if (!accessableTiles.Contains(neighbour) && neighbour != gridPos)
                return;

            if (Vector3.Distance(GridStaticFunctions.CalcSquareWorldPos(neighbour), worldpoint) < smallestDistance) {
                smallestDistance = Vector3.Distance(GridStaticFunctions.CalcSquareWorldPos(neighbour), worldpoint);
                closestTile = neighbour;
            }
        });

        if (closestTile != Vector2Int.zero)
            return closestTile;

        return Vector2Int.zero;
    }
}