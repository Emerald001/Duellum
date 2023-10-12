using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {
    private const int SKEWED_POWER = 1;

    [SerializeField] private int tileAmount;
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
        connectionDict.Add(new(0, 0), new(0, 1));

        void AddNewTile(Vector2Int currentPos, Vector2Int direction) {
            Vector2Int newTile = currentPos + direction;
            if (closedSet.Contains(newTile) || openSet.Contains(newTile))
                return;

            openSet.Add(newTile);
            if (!connectionDict.ContainsKey(newTile))
                connectionDict.Add(newTile, currentPos);
        }
        
        while (openSet.Count > 0) {
            if (openSet.Count + closedSet.Count >= tileAmount)
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

                    if (connections.x > 0)
                        AddNewTile(tile, new Vector2Int(1, 0));
                    if (connections.y > 0)
                        AddNewTile(tile, new Vector2Int(-1, 0));
                    if (connections.z > 0)
                        AddNewTile(tile, new Vector2Int(0, -1));
                    if (connections.w > 0)
                        AddNewTile(tile, new Vector2Int(0, 1));

                    if (openSet.Contains(tile))
                        openSet.Remove(tile);

                    closedSet.Add(tile);
                    dungeon.Add(tile, room);
                    index++;
                }
            }
        }

        for (int i = 0; i < openSet.Count; i++) {
            Vector2Int currentPos = openSet[i];
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
            List<int> availableIndices = tile.GridPositionsPerIndex.Keys
                .Where(r => CheckForFit(tile, position, r) && CheckForConnections(tile, connectionDir, r))
                .ToList();

            if (availableIndices.Count > 0)
                tile.CurrentIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            else
                availableTiles.Remove(tile);
        }

        int skewedRandomValue = Mathf.RoundToInt(Mathf.Pow(Random.value, SKEWED_POWER) * (availableTiles.Count - 1) + 0);
        DungeonRoomTile room = availableTiles[skewedRandomValue];

        GameObject spawnedRoom = Instantiate(room.prefab);
        spawnedRoom.name = spawnedRoom.name + "/" + room.name;
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

        return 
            connectionDirection.x > 0 && connections.x > 0 ||
            connectionDirection.x < 0 && connections.y > 0 ||
            connectionDirection.y > 0 && connections.z > 0 ||
            connectionDirection.y < 0 && connections.w > 0;
    }

    private bool CheckForFit(DungeonRoomTile room, Vector2Int position, int index) {
        Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[index];

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
