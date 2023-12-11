using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridStaticSelectors {
    public static List<Vector2Int> GetPositions(Selector selector, Vector2Int startPos) {
        List<Vector2Int> result = new();

        switch (selector.type) {
            case SelectorType.SingleTile:
                result.Add(startPos);
                break;
            case SelectorType.Circle:
                result = GetAvailableTilesCircle(selector, startPos);
                break;
            case SelectorType.Line:
                result = selector.isHex ? GetAvailableTilesLineHex(selector, startPos) : GetAvailableTilesLineSquare(selector, startPos);
                break;

            case SelectorType.FriendlyUnits:
            case SelectorType.EnemyUnits:
            case SelectorType.AllUnits:
                result = GetAvailableTilesUnits(selector, startPos);
                break;

            case SelectorType.AllTiles:
                result = GetAllTiles(selector);
                break;

            default:
                Debug.LogError($"{selector.type} not yet Implemented");
                break;
        }

        return result;
    }

    private static List<Vector2Int> GetAvailableTilesCircle(Selector selector, Vector2Int startPos) {
        List<Vector2Int> result = new();

        if (selector.isHex)
            GridStaticFunctions.RippleThroughGridPositions(startPos, selector.range, (currentPos, index) => {
                result.Add(currentPos);
            });
        else
            GridStaticFunctions.RippleThroughSquareGridPositions(startPos, selector.range, (currentPos, index) => {
                result.Add(currentPos);
            });

        if (!selector.includeCentreTile)
            result.Remove(startPos);

        return result;
    }

    private static List<Vector2Int> GetAvailableTilesLineHex(Selector selector, Vector2Int startPos) {
        List<Vector2Int> result = new() {
            startPos
        };

        if (selector.AllDirections)
            for (int j = 0; j < 6; j++) {
                result.Add(startPos);

                for (int i = 0; i < selector.range; i++) {
                    if (GridStaticFunctions.TryGetHexNeighbour(result[^1], j, out Vector2Int pos))
                        result.Add(pos);
                }

                result.Remove(startPos);
            }
        else
            for (int i = 0; i < selector.range; i++) {
                if (GridStaticFunctions.TryGetHexNeighbour(result[^1], selector.rotIndex, out Vector2Int pos))
                    result.Add(pos);
            }

        if (!selector.includeCentreTile)
            result.Remove(startPos);

        return result;
    }

    private static List<Vector2Int> GetAvailableTilesLineSquare(Selector selector, Vector2Int startPos) {
        List<Vector2Int> result = new() {
            startPos
        };

        if (selector.AllDirections)
            for (int j = 0; j < 4; j++) {
                result.Add(startPos);

                for (int i = 0; i < selector.range; i++) {
                    if (GridStaticFunctions.TryGetSquareNeighbour(result[^1], j, out Vector2Int pos))
                        result.Add(pos);
                }

                result.Remove(startPos);
            }
        else
            for (int i = 0; i < selector.range; i++) {
                if (GridStaticFunctions.TryGetSquareNeighbour(result[^1], selector.rotIndex, out Vector2Int pos))
                    result.Add(pos);
            }

        if (!selector.includeCentreTile)
            result.Remove(startPos);

        return result;
    }

    private static List<Vector2Int> GetAvailableTilesUnits(Selector selector, Vector2Int startpos) {
        List<Vector2Int> result = new();

        if (selector.includeCentreTile)
            result.Add(startpos);

        switch (selector.type) {
            case SelectorType.AllUnits:
                return UnitStaticManager.LivingUnitsInPlay.Select(unit => UnitStaticManager.GetUnitPosition(unit)).ToList();
            case SelectorType.FriendlyUnits:
                return UnitStaticManager.PlayerUnitsInPlay.Select(unit => UnitStaticManager.GetUnitPosition(unit)).ToList();
            case SelectorType.EnemyUnits:
                return UnitStaticManager.EnemyUnitsInPlay.Select(unit => UnitStaticManager.GetUnitPosition(unit)).ToList();

            default:
                return null;
        }
    }

    private static List<Vector2Int> GetAllTiles(Selector selector) {
        List<Vector2Int> result = GridStaticFunctions.Grid.Values
            .Where(tile => tile.Type == TileType.Normal)
            .Select(tile => tile.GridPos).ToList();

        if (selector.includeWater) {
            result.AddRange(GridStaticFunctions.Grid.Values
                .Where(tile => tile.Type == TileType.Water)
                .Select(tile => tile.GridPos));
        }
        if (selector.includeCover) {
            result.AddRange(GridStaticFunctions.Grid.Values
                .Where(tile => tile.Type == TileType.Cover)
                .Select(tile => tile.GridPos));
        }
        if (selector.excludeUnits) {
            foreach (var item in UnitStaticManager.UnitPositions) {
                if (result.Contains(item.Value))
                    result.Remove(item.Value);
            }
        }

        return result;
    }
}
