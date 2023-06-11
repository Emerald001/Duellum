using System.Collections.Generic;
using UnityEngine;

public static class GridStaticSelectors
{
    public static List<Vector2Int> GetPositions(Selector selector, Vector2Int startPos) {
        List<Vector2Int> result = new();

        switch (selector.type) {
            case SelectorType.SingleTile:
                result.Add(startPos);
                break;
            case SelectorType.Circle:
                result = GetAvailableTilesCircle(startPos, selector.range);
                break;
            case SelectorType.Line:
                result = GetAvailableTilesLine(startPos, selector.range, selector.rotIndex);
                break;

            default:
                Debug.LogError($"{selector.type} not yet Implemented");
                break;
        }

        return result;
    }

    private static List<Vector2Int> GetAvailableTilesCircle(Vector2Int startPos, int range) {
        List<Vector2Int> result = new();

        GridStaticFunctions.RippleThroughGridPositions(startPos, range, (currentPos, index) => {
            result.Add(currentPos);
        });

        return result;
    }

    private static List<Vector2Int> GetAvailableTilesLine(Vector2Int startPos, int range, int index) {
        List<Vector2Int> result = new() {
            startPos
        };

        if (index > 6) {
            Debug.LogError("Index is above 6, this is not possible!");
            return null;
        }

        if (index == 6)
            for (int j = 0; j < 6; j++) {
                result.Add(startPos);

                for (int i = 0; i < range; i++) {
                    if (GridStaticFunctions.TryGetHexNeighbour(result[^1], j, out Vector2Int pos))
                        result.Add(pos);
                }

                result.Remove(startPos);
            }
        else
            for (int i = 0; i < range; i++) {
                if (GridStaticFunctions.TryGetHexNeighbour(result[^1], index, out Vector2Int pos))
                    result.Add(pos);
            }
        result.Remove(startPos);

        return result;
    }
}
