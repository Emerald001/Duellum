using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitMovementComponent {
    public List<Vector2Int> AccessableTiles => currentAccessableTiles;
    public bool IsDone { get; private set; }

    private readonly Dictionary<Vector2Int, Vector2Int> parentDictionary = new();
    private readonly List<Vector2Int> currentAccessableTiles = new();
    private Vector2Int unitPosition;

    public void FindAccessibleTiles(Vector2Int gridPos, int speedValue) {
        unitPosition = gridPos;

        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new(); // Use HashSet for faster membership checks

        openList.Add(gridPos);
        for (int i = 0; i < speedValue; i++) {
            foreach (var currentPos in openList.ToList()) { // Use ToList() to avoid modification during iteration
                GridStaticFunctions.RippleThroughSquareGridPositions(currentPos, 2, (neighbour, count) => {
                    if (neighbour == currentPos)
                        return;

                    if (GridStaticFunctions.TryGetUnitFromGridPos(neighbour, out var tmp))
                        return;

                    if (!GridStaticFunctions.Grid[neighbour].CompareTag("WalkableTile") ||
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
            openList.AddRange(layerList); // Use ForEach for adding elements from one list to another
            layerList.Clear();
        }

        GridStaticFunctions.HighlightTiles(currentAccessableTiles);
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
