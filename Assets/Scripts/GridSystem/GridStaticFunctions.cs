using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GridStaticFunctions {
    public static int CONST_INT = -999;
    public static Vector2Int CONST_EMPTY = new(12345, 12345);

    public static int TilesPerRoom = 5;
    public static int BattleMapSize = 11;

    public static readonly Vector2Int[] directCubeNeighbours = {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
    };

    public static readonly Vector2Int[] diagonalCubeNeighbours = {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    private static readonly float[] lookDirections = {
        90,
        0,
        270,
        180,
    };

    public static Dictionary<Vector2Int, GameObject> CardPositions { get; set; } = new();
    public static Dictionary<Vector2Int, Tile> Grid { get; set; } = new();
    public static Dictionary<Vector2Int, Tile> CurrentBattleGrid { get; set; } = new();
    public static Dictionary<Vector2Int, RoomComponent> Dungeon { get; set; } = new();
    public static Dictionary<Vector2Int, Vector4> DungeonConnections { get; set; } = new();

    public static List<Vector2Int> PlayerSpawnPositions { get; set; } = new();
    public static List<Vector2Int> EnemySpawnPositions { get; set; } = new();

    public static void Reset() {
        CardPositions.Clear();
        Grid.Clear();
        PlayerSpawnPositions.Clear();
        EnemySpawnPositions.Clear();
    }

    public static Vector3 CalcWorldPos(Vector2Int gridPos) {
        float x = gridPos.x - TilesPerRoom / 2;
        float y = Grid[gridPos].Height;
        float z = gridPos.y - TilesPerRoom / 2;

        return new Vector3(x, y, z);
    }

    public static Vector2Int GetGridPosFromTileGameObject(GameObject valueVar) {
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

    public static void RippleThroughFullGridPositions(Vector2Int spawnPos, float range, Action<Vector2Int, int> action, bool hasCreatedGrid = true) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        List<Vector2Int> neighbours = new();
        neighbours.AddRange(directCubeNeighbours);
        neighbours.AddRange(diagonalCubeNeighbours);

        openList.Add(spawnPos);
        for (int i = 0; i < range; i++) {
            for (int j = 0; j < openList.Count; j++) {
                Vector2Int currentPos = openList[j];

                if (i < range - 1) {
                    for (int k = 0; k < neighbours.Count; k++) {
                        Vector2Int neighbour = currentPos + neighbours[k];
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

    public static void HighlightBattleTiles(List<Vector2Int> tiles, HighlightType type) {
        foreach (var tile in tiles)
            CurrentBattleGrid[tile].SetHighlight(type);
    }

    public static void ResetAllTileColors() {
        foreach (var tile in Grid.Values)
            tile.SetHighlight(HighlightType.None);
    }

    public static void ResetBattleTileColors() {
        foreach (var tile in CurrentBattleGrid.Values)
            tile.SetHighlight(HighlightType.None);
    }

    public static void ReplaceTile(Tile hexPrefab, params Vector2Int[] hexPositions) {
        foreach (var hex in hexPositions) {
            UnityEngine.Object.Destroy(Grid[hex].gameObject);

            Grid[hex] = UnityEngine.Object.Instantiate(hexPrefab);
            Grid[hex].transform.position = CalcWorldPos(hex);
        }
    }

    public static void SetBattleGrid(List<Vector2Int> positions) {
        CurrentBattleGrid.Clear();

        foreach(var position in positions) {
            if (!Grid.ContainsKey(position))
                continue;

            CurrentBattleGrid.Add(position, Grid[position]);
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
        var result = CurrentBattleGrid.Keys.Where(hex => Grid[hex].Type == TileType.Normal).ToList();
        var unitPositions = UnitStaticManager.UnitPositions.Values.ToList();

        for (int i = result.Count - 1; i >= 0; i--) {
            if (unitPositions.Contains(result[i])) {
                result.RemoveAt(i);
                continue;
            }

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

    public static bool TryGetNeighbour(Vector2Int startPos, int dirIndex, out Vector2Int result) {
        if (Grid.TryGetValue(startPos + directCubeNeighbours[dirIndex], out Tile hex)) {
            result = hex.GridPos;
            return true;
        }

        result = CONST_EMPTY;
        return false;
    }
}
