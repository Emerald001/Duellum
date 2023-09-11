using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class GridStaticFunctions {
    public static Vector2Int CONST_EMPTY = new(12345, 12345);
    public static Color CONST_HIGHLIGHT_COLOR = new(50, 50, 50);

    private static readonly Vector2Int[] evenNeighbours = {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
    };

    private static readonly Vector2Int[] unevenNeighbours = {
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
    };

    private static readonly Vector2Int[] cubeNeighbours = {
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
    };

    public static float HexWidth { get; set; }
    public static float HexHeight { get; set; }

    public static float SquareWidth { get; set; }
    public static float SquareHeight { get; set; }

    public static int GridWidth { get; set; }
    public static int GridHeight { get; set; }
    public static float GridGap { get; set; }

    public static Vector3 StartPos { get; set; }
    public static Dictionary<Vector2Int, Hex> Grid { get; set; } = new();
    public static List<Vector2Int> PlayerSpawnPos { get; set; } = new();
    public static List<Vector2Int> EnemySpawnPos { get; set; } = new();

    public static List<Tuple<Vector2Int, UnitController>> UnitPositions { get; set; } = new();

    public static Vector3 CalcHexWorldPos(Vector2Int gridPos) {
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = HexWidth / 2;

        float x = StartPos.x + gridPos.x * HexWidth + offset;
        float z = StartPos.z - gridPos.y * HexHeight * .75f;

        return new Vector3(x, 0, z);
    }

    public static Vector3 CalcSquareWorldPos(Vector2Int gridpos) {
        float x = gridpos.x - (GridWidth / 2 + (GridGap * GridWidth) / 2) + GridGap * gridpos.x;
        float z = gridpos.y - (GridHeight / 2 + (GridGap * GridHeight) / 2) + GridGap * gridpos.y;

        return new Vector3(x, 0, z);
    }

    public static Vector2Int GetGridPosFromHexGameObject(GameObject valueVar) {
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
                Vector2Int[] listToUse = currentPos.y % 2 != 0 ? unevenNeighbours : evenNeighbours;

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
            openList.AddRange(layerList);
            layerList.Clear();
        }
    }

    public static void RippleThroughSquareGridPositions(Vector2Int spawnPos, float range, System.Action<Vector2Int, int> action, bool hasCreatedGrid = true) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        openList.Add(spawnPos);
        for (int i = 0; i < range; i++) {
            for (int j = 0; j < openList.Count; j++) {
                Vector2Int currentPos = openList[j];

                if (i < range - 1) {
                    for (int k = 0; k < 8; k++) {
                        Vector2Int neighbour = currentPos + cubeNeighbours[k];
                        if (openList.Contains(neighbour) || closedList.Contains(neighbour) || layerList.Contains(neighbour) || (hasCreatedGrid && !Grid.ContainsKey(neighbour)))
                            continue;

                        layerList.Add(neighbour);
                    }
                }

                // Invokes on every tile found
                action.Invoke(currentPos, i);
                closedList.Add(openList[j]);
            }

            openList.Clear();
            openList.AddRange(layerList);
            layerList.Clear();
        }
    }

    public static void HighlightTiles(List<Vector2Int> tiles) {
        foreach (var tile in tiles) {
            var color = Grid[tile].GivenColor.color + CONST_HIGHLIGHT_COLOR;
            Material mat = new(Grid[tile].GetComponent<Renderer>().material) {
                color = color
            };

            Grid[tile].SetColor(mat);
        }
    }

    public static void ResetTileColors() {
        foreach (var tile in Grid.Values)
            tile.SetColor();
    }

    public static bool TryGetHexNeighbour(Vector2Int startPos, int dirIndex, out Vector2Int result) {
        Vector2Int[] listToUse = startPos.y % 2 != 0 ? unevenNeighbours : evenNeighbours;
        if (Grid.TryGetValue(startPos + listToUse[dirIndex], out Hex hex)) {
            result = hex.GridPos;
            return true;
        }

        result = CONST_EMPTY;
        return false;
    }

    public static bool TryGetUnitFromGridPos(Vector2Int position, out UnitController unit) {
        foreach (var item in UnitPositions) {
            if (item.Item1 == position) {
                unit = item.Item2;
                return true;
            }
        }

        unit = null;
        return false;
    }
}