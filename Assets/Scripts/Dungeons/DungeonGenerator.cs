using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {
    private const int SKEWED_POWER = 1;

    [SerializeField] private int roomAmount;
    [SerializeField] private int tileSize;

    [SerializeField] private List<DungeonRoomTile> rooms;
    [SerializeField] private List<DungeonRoomTile> endRooms;

    private readonly Dictionary<Vector2Int, DungeonRoomTile> dungeon = new();

    private void Awake() {
        SpawnRooms();
    }

    private void SpawnRooms() {
        // Key; New Tile | Value; Last Tile
        Dictionary<Vector2Int, Vector2Int> connectionDict = new();

        List<Vector2Int> openSet = new();
        List<Vector2Int> closedSet = new();

        openSet.Add(new(0, 0));
        connectionDict.Add(new(0, 0), new(0, 0));

        while (openSet.Count > 0) {
            if (openSet.Count + closedSet.Count >= roomAmount)
                break;

            Vector2Int currentPos = openSet[0];
            Vector2Int connectionDirection = currentPos - connectionDict[currentPos];
            DungeonRoomTile room = SpawnRoom(rooms, currentPos, connectionDirection);

            Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];
            int index = 0;
            for (int x = 0; x < room.size.x; x++) {
                for (int y = 0; y < room.size.y; y++) {
                    Vector2Int tile = currentPos + new Vector2Int(x, y) - offset;
                    Vector4 connections = room.connections[index];

                    if (connections.x > 0) {
                        if (!closedSet.Contains(tile + new Vector2Int(1, 0)) && !openSet.Contains(tile + new Vector2Int(1, 0))) {
                            openSet.Add(tile + new Vector2Int(1, 0));
                            if (!connectionDict.ContainsKey(tile + new Vector2Int(1, 0)))
                                connectionDict.Add(tile + new Vector2Int(1, 0), tile);
                        }
                    }
                    if (connections.y > 0) {
                        if (!closedSet.Contains(tile + new Vector2Int(-1, 0)) && !openSet.Contains(tile + new Vector2Int(-1, 0))) {
                            openSet.Add(tile + new Vector2Int(-1, 0));
                            connectionDict.Add(tile + new Vector2Int(-1, 0), tile);
                        }
                    }
                    if (connections.z > 0) {
                        if (!closedSet.Contains(tile + new Vector2Int(0, 1)) && !openSet.Contains(tile + new Vector2Int(0, 1))) {
                            openSet.Add(tile + new Vector2Int(0, 1));
                            connectionDict.Add(tile + new Vector2Int(0, 1), tile);
                        }
                    }
                    if (connections.w > 0) {
                        if (!closedSet.Contains(tile + new Vector2Int(0, -1)) && !openSet.Contains(tile + new Vector2Int(0, -1))) {
                            openSet.Add(tile + new Vector2Int(0, -1));
                            connectionDict.Add(tile + new Vector2Int(0, -1), tile);
                        }
                    }

                    if (openSet.Contains(tile))
                        openSet.Remove(tile);

                    dungeon.Add(tile, room);
                    index++;
                }
            }

            closedSet.Add(currentPos);
            foreach (var item in openSet) {
                if (closedSet.Contains(item))
                    Debug.Log("Something Is Wrong");
            }
        }

        for (int i = 0; i < openSet.Count; i++) {
            Vector2Int currentPos = openSet[0];
            Vector2Int connectionDirection = currentPos - connectionDict[currentPos];
            DungeonRoomTile room = SpawnRoom(endRooms, currentPos, connectionDirection);

            Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];
            for (int x = 0; x < room.size.x; x++) {
                for (int y = 0; y < room.size.y; y++) {
                    Vector2Int tile = currentPos + new Vector2Int(x, y) - offset;
                    dungeon.Add(tile, room);
                }
            }
        }

        openSet.Clear();
        closedSet.Clear();
    }

    private DungeonRoomTile SpawnRoom(List<DungeonRoomTile> listToUse, Vector2Int position, Vector2Int connectionDir) {
        // Add all rotation variaties of each room
        List<DungeonRoomTile> availableTiles = new(listToUse);
        availableTiles.ForEach(room => { room.Init(); });

        for (int i = availableTiles.Count - 1; i >= 0; i--) {
            DungeonRoomTile tile = availableTiles[i];

            List<int> availableIndices = new(tile.GridPositionsPerIndex.Keys);
            for (int r = 0; r < tile.GridPositionsPerIndex.Count; r++) {
                Vector2Int offset = Vector2Int.zero - tile.GridPositionsPerIndex[r];

                if (!CheckForFit(tile, offset, position) || CheckForConnections(tile, connectionDir, r))
                    availableIndices.Remove(r);
            }

            if (availableIndices.Count > 0)
                tile.CurrentIndex = availableIndices[0];
        }

        int skewedRandomValue = Mathf.RoundToInt(Mathf.Pow(UnityEngine.Random.value, SKEWED_POWER) * (availableTiles.Count - 1) + 0);
        DungeonRoomTile room = availableTiles[skewedRandomValue];

        GameObject spawnedRoom = Instantiate(room.prefab);
        spawnedRoom.transform.position = CalculateWorldPosition(position, room);

        return room;
    }

    private Vector3 CalculateWorldPosition(Vector2Int position, DungeonRoomTile room) {
        Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];
        Vector3 worldPosition = new();

        for (int x = 0; x < room.size.x; x++) {
            for (int y = 0; y < room.size.y; y++) {
                Vector2Int tile = position + new Vector2Int(x, y) - offset;
                worldPosition += new Vector3(tile.x * tileSize, 0, tile.y * tileSize);
            }
        }

        return worldPosition / (room.size.x * room.size.y);
    }

    private bool CheckForConnections(DungeonRoomTile room, Vector2Int connectionDirection, int index) {
        Vector4 connections = room.connections[index];
        if (connectionDirection.x > 0 && connections.x > 0 ||
            connectionDirection.x < 0 && connections.y > 0 ||
            connectionDirection.y > 0 && connections.z > 0 ||
            connectionDirection.y < 0 && connections.w > 0)
            return true;

        return false;
    }

    private bool CheckForFit(DungeonRoomTile room, Vector2Int offset, Vector2Int position) {
        for (int x = 0; x < room.size.x; x++) {
            for (int y = 0; y < room.size.y; y++) {
                Vector2Int roomTile = new Vector2Int(x, y) - offset;

                if (dungeon.ContainsKey(position + roomTile))
                    return false;
            }
        }

        return true;
    }
}

[Serializable]
public class DungeonRoomTile {
    public string name;
    public Vector2Int size;
    public GameObject prefab;

    public List<Vector4> connections;

    public Dictionary<int, Vector2Int> GridPositionsPerIndex { get; private set; }
    public int CurrentIndex { get; set; }

    public void Init() {
        GridPositionsPerIndex = new();

        int counter = 0;
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                GridPositionsPerIndex.Add(counter, new Vector2Int(x, y));

                counter++;
            }
        }
    }
}