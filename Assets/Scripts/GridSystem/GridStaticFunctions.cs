using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private static readonly Vector2Int[] directCubeNeighbours = {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
    };

    private static readonly float[] lookDirections = {
        90,
        0,
        270,
        180,
    };

    public static float HexWidth { get; set; }
    public static float HexHeight { get; set; }

    public static float SquareWidth { get; set; }
    public static float SquareHeight { get; set; }

    public static int GridWidth { get; set; }
    public static int GridHeight { get; set; }
    public static float GridGap { get; set; }

    public static Vector3 StartPos { get; set; }

    public static Dictionary<Vector2Int, GameObject> CardPositions { get; set; } = new();
    public static Dictionary<Vector2Int, Tile> Grid { get; set; } = new();
    public static List<Vector2Int> PlayerSpawnPos { get; set; } = new();
    public static List<Vector2Int> EnemySpawnPos { get; set; } = new();
    public static List<GameObject> SpawnCubes { get; set; } = new();

    public static void Reset() {
        CardPositions.Clear();
        Grid.Clear();
        PlayerSpawnPos.Clear();
        EnemySpawnPos.Clear();
        SpawnCubes.Clear();
    }

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

    public static void RippleThroughGridPositions(Vector2Int spawnPos, float range, Action<Vector2Int, int> action, bool hasCreatedGrid = true) {
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

    public static void RippleThroughSquareGridPositions(Vector2Int spawnPos, float range, Action<Vector2Int, int> action, bool hasCreatedGrid = true) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        openList.Add(spawnPos);
        for (int i = 0; i < range; i++) {
            for (int j = 0; j < openList.Count; j++) {
                Vector2Int currentPos = openList[j];

                if (i < range - 1) {
                    for (int k = 0; k < directCubeNeighbours.Length; k++) {
                        Vector2Int neighbour = currentPos + directCubeNeighbours[k];
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

    public static void HighlightTiles(List<Vector2Int> tiles, HighlightType type) {
        foreach (var tile in tiles)
            Grid[tile].SetHighlight(type);
    }

    public static void ResetTileColors() {
        foreach (var tile in Grid.Values)
            tile.SetHighlight(HighlightType.None);
    }

    public static void ReplaceHex(Tile hexPrefab, params Vector2Int[] hexPositions) {
        foreach (var hex in hexPositions) {
            UnityEngine.Object.Destroy(Grid[hex].gameObject);

            Grid[hex] = UnityEngine.Object.Instantiate(hexPrefab);
            Grid[hex].transform.position = CalcSquareWorldPos(hex);
        }
    }

    public static Tile GetTileFromPosition(Vector2Int position) {
        if (Grid.ContainsKey(position))
            return Grid[position];

        return null;
    }

    public static Vector2Int GetVector2RotationFromDirection(Vector3 dir) {
        Vector2Int result = new(
            Mathf.Min(1, Mathf.Max(-1, Mathf.RoundToInt(dir.x))),
            Mathf.Min(1, Mathf.Max(-1, Mathf.RoundToInt(dir.z))));

        return result;
    }

    public static List<Vector2Int> GetAllOpenGridPositions() {
        var result = Grid.Keys.Where(hex => Grid[hex].Type == TileType.Normal).ToList();
        var unitPositions = UnitStaticManager.UnitPositions.Values.ToList();

        for (int i = result.Count - 1; i >= 0; i--) {
            if (unitPositions.Contains(result[i]))
                result.RemoveAt(i);

            if (CardPositions.ContainsKey(result[i]))
                result.RemoveAt(i);
        }

        return result;
    }

    public static float GetRotationFromVector2Direction(Vector2Int dir) {
        for (int i = 0; i < directCubeNeighbours.Length; i++) {
            Vector2Int direction = directCubeNeighbours[i];
            if (direction == dir)
                return lookDirections[i];
        }
        return 0;
    }

    public static bool TryGetHexNeighbour(Vector2Int startPos, int dirIndex, out Vector2Int result) {
        Vector2Int[] listToUse = startPos.y % 2 != 0 ? unevenNeighbours : evenNeighbours;
        if (Grid.TryGetValue(startPos + listToUse[dirIndex], out Tile hex)) {
            result = hex.GridPos;
            return true;
        }

        result = CONST_EMPTY;
        return false;
    }

    public static bool TryGetSquareNeighbour(Vector2Int startPos, int dirIndex, out Vector2Int result) {
        if (Grid.TryGetValue(startPos + directCubeNeighbours[dirIndex], out Tile hex)) {
            result = hex.GridPos;
            return true;
        }

        result = CONST_EMPTY;
        return false;
    }
}
