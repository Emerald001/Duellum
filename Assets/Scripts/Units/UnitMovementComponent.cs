using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitMovementComponent {
    public List<Vector2Int> AccessableTiles => currentAccessableTiles;

    private readonly Dictionary<Vector2Int, Vector2Int> parentDictionary = new();
    private readonly List<Vector2Int> currentAccessableTiles = new();
    private Vector2Int unitPosition;

    public void FindAccessibleTiles(Vector2Int gridPos, int speedValue) {
        unitPosition = gridPos;

        parentDictionary.Clear();
        currentAccessableTiles.Clear();

        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        openList.Add(gridPos);
        for (int i = 0; i < speedValue; i++) {
            foreach (var currentPos in openList.ToList()) {
                GridStaticFunctions.RippleThroughSquareGridPositions(currentPos, 2, (neighbour, count) => {
                    if (neighbour == currentPos)
                        return;

                    if (GridStaticFunctions.TryGetUnitFromGridPos(neighbour, out var tmp))
                        return;

                    if (GridStaticFunctions.Grid[neighbour].Type != HexType.Normal ||
                        openList.Contains(neighbour) ||
                        closedList.Contains(neighbour) ||
                        layerList.Contains(neighbour))
                        return;

                    layerList.Add(neighbour);
                    currentAccessableTiles.Add(neighbour);
                    parentDictionary[neighbour] = currentPos;
                });

                closedList.Add(currentPos);
            }

            openList.Clear();
            openList.AddRange(layerList);
            layerList.Clear();
        }
    }

    public List<Vector2Int> GetPath(Vector2Int endPos) {
        List<Vector2Int> path = new();
        Vector2Int currentPosition = endPos;

        while (currentPosition != unitPosition) {
            path.Add(currentPosition);
            currentPosition = parentDictionary[currentPosition];
        }

        path.Add(currentPosition);
        path.Reverse();

        return path;
    }
}
