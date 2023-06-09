using System.Collections.Generic;
using UnityEngine;

public static class GridStaticFunctions {
    public static Vector2Int CONST_EMPTY = new(12345, 12345);

    private static readonly Vector2Int[] evenNeighbours = {
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
        };

    private static readonly Vector2Int[] unevenNeighbours = {
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };

    public static float HexWidth { get; set; }
    public static float HexHeight { get; set; }
    public static Vector3 StartPos { get; set; }
    public static Dictionary<Vector2Int, Hex> Grid { get; set; } = new();

    public static Vector3 CalcWorldPos(Vector2Int gridPos) {
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = HexWidth / 2;

        float x = StartPos.x + gridPos.x * HexWidth + offset;
        float z = StartPos.z - gridPos.y * HexHeight * .75f;

        return new Vector3(x, 0, z);
    }

    public static Vector2Int GetGridPosFromWorldPos(GameObject valueVar) {
        foreach (Vector2Int keyVar in Grid.Keys) {
            if (Grid[keyVar].gameObject != valueVar)
                continue;
            return keyVar;
        }
        return CONST_EMPTY;
    }

    public static void RippleThroughGridPositions(Vector2Int spawnPos, float range, System.Action<Vector2Int, int> action, bool hasCreatedGrid = true) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        openList.Add(spawnPos);
        for (int i = 0; i < range; i++) {
            for (int j = 0; j < openList.Count; j++) {
                Vector2Int currentPos = openList[j];
                Vector2Int[] listToUse;

                if (currentPos.y % 2 != 0)
                    listToUse = unevenNeighbours;
                else
                    listToUse = evenNeighbours;

                for (int k = 0; k < 6; k++) {
                    Vector2Int neighbour = currentPos + listToUse[k];
                    if (openList.Contains(neighbour) || closedList.Contains(neighbour) || layerList.Contains(neighbour) || (hasCreatedGrid && !Grid.ContainsKey(neighbour)))
                        continue;

                    layerList.Add(neighbour);
                }

                // Invokes on every tile found
                action.Invoke(currentPos, i);
                closedList.Add(openList[j]);
            }

            openList.Clear();
            for (int j = 0; j < layerList.Count; j++)
                openList.Add(layerList[j]);

            layerList.Clear();
        }
    }
}