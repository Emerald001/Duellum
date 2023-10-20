using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {
    private const float SKEWED_POWER = .3f;

    [SerializeField] private int roomAmount;
    [SerializeField] private int bigRoomAmount;
    [SerializeField] private int tileSize;

    [SerializeField] private List<DungeonRoomTile> rooms;
    [SerializeField] private List<DungeonRoomTile> endRooms;

    [SerializeField] private bool generateInstantly;
    [SerializeField] private float generationSpeed;

    private readonly Dictionary<Vector2Int, Vector4> dungeon = new();

    private void Awake() {
        StartCoroutine(SpawnRooms());
    }

    private IEnumerator SpawnRooms() {
        List<Vector2Int> openSet = new() { new(0, 0) };
        Dictionary<Vector2Int, GameObject> debugObjects = new();

        void AddNewTile(Vector2Int currentPos, Vector2Int direction) {
            Vector2Int newTile = currentPos + direction;
            if (dungeon.ContainsKey(newTile) || openSet.Contains(newTile))
                return;

            openSet.Add(newTile);
            if (!generateInstantly)
                CreateDebugCube(newTile);
        }

        void CreateDebugCube(Vector2Int position) {
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.transform.localScale = new Vector3(.1f, .1f, .1f);
            tmp.transform.localPosition = new Vector3(position.x * tileSize, 1, position.y * tileSize);
            tmp.GetComponent<Renderer>().material.color = Color.green;
            debugObjects.Add(position, tmp);
        }

        int roomsSpawned = 0;
        while (openSet.Count > 0) {
            if (openSet.Count + roomsSpawned >= roomAmount)
                break;

            Vector2Int currentPos = openSet[0];
            DungeonRoomTile room = SpawnRoom(rooms, currentPos);
            roomsSpawned++;

            if (!generateInstantly) {
                if (debugObjects.ContainsKey(currentPos))
                    debugObjects[currentPos].GetComponent<Renderer>().material.color = Color.yellow;
                yield return new WaitForSeconds(generationSpeed);
            }

            Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];
            int index = 0;
            for (int x = 0; x < room.size.x; x++) {
                for (int y = 0; y < room.size.y; y++) {
                    Vector2Int tile = currentPos + new Vector2Int(x, y) + offset;
                    Vector4 connections = room.connections[index];

                    if (connections.x > 0)
                        AddNewTile(tile, new Vector2Int(1, 0));
                    if (connections.y > 0)
                        AddNewTile(tile, new Vector2Int(-1, 0));
                    if (connections.z > 0)
                        AddNewTile(tile, new Vector2Int(0, 1));
                    if (connections.w > 0)
                        AddNewTile(tile, new Vector2Int(0, -1));

                    if (openSet.Contains(tile))
                        openSet.Remove(tile);

                    if (!generateInstantly) {
                        if (debugObjects.ContainsKey(tile))
                            debugObjects[tile].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(generationSpeed);
                    }

                    dungeon.Add(tile, connections);
                    index++;
                }
            }
        }

        foreach (var currentPos in openSet) {
            DungeonRoomTile room = SpawnRoom(endRooms, currentPos, true);

            if (!generateInstantly) {
                if (debugObjects.ContainsKey(currentPos))
                    debugObjects[currentPos].GetComponent<Renderer>().material.color = Color.yellow;
                yield return new WaitForSeconds(generationSpeed);
            }

            Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];
            int index = 0;
            for (int x = 0; x < room.size.x; x++) {
                for (int y = 0; y < room.size.y; y++) {
                    Vector2Int tile = currentPos + new Vector2Int(x, y) + offset;
                    dungeon.Add(tile, room.connections[index]);

                    if (!generateInstantly) {
                        if (debugObjects.ContainsKey(tile))
                            debugObjects[tile].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(generationSpeed);
                    }
                    index++;
                }
            }
        }

        openSet.Clear();
    }

    private DungeonRoomTile SpawnRoom(List<DungeonRoomTile> listToUse, Vector2Int position, bool spawnFirst = false) {
        List<DungeonRoomTile> availableTiles = listToUse
        .SelectMany(item => Enumerable.Range(0, (item.size.x > 1 || item.size.y > 1) ? 1 : 4)
            .Select(i => {
                DungeonRoomTile roomTile = new(item.name, item.size, item.prefab, item.connections);
                roomTile.Init(i);

                return roomTile;
            }))
        .Where(tile => {
            if (bigRoomAmount < 1 && (tile.size.x > 1 || tile.size.y > 1))
                return false;

            List<int> availableIndices = tile.GridPositionsPerIndex.Keys
                .Where(r => CheckForFit(tile, position, r))
                .ToList();

            if (availableIndices.Count > 0) {
                tile.CurrentIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                return true;
            }

            return false;
        })
        .ToList();

        int index = spawnFirst ? 0 : Mathf.RoundToInt(Mathf.Pow(Random.value, SKEWED_POWER) * (availableTiles.Count - 1) + 0);
        DungeonRoomTile room = availableTiles[index];

        bigRoomAmount -= (room.size.x > 1 || room.size.y > 1) ? 1 : 0;

        GameObject spawnedRoom = Instantiate(room.prefab);
        spawnedRoom.name = room.name;
        spawnedRoom.transform.position = CalculateWorldPosition(position, room);
        spawnedRoom.transform.eulerAngles = new(0, room.RotationIndex * 90, 0);

        return room;
    }

    private Vector3 CalculateWorldPosition(Vector2Int position, DungeonRoomTile room) {
        Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];

        Vector3 worldPosition = new();
        for (int x = 0; x < room.size.x; x++) {
            for (int y = 0; y < room.size.y; y++)
                worldPosition += new Vector3((position.x + x + offset.x) * tileSize, 0, (position.y + y + offset.y) * tileSize);
        }

        return worldPosition / (room.size.x * room.size.y);
    }

    private bool CheckForFit(DungeonRoomTile room, Vector2Int spawnPos, int index) {
        int i = 0;
        for (int x = 0; x < room.size.x; x++) {
            for (int y = 0; y < room.size.y; y++) {
                Vector2Int roomTile = spawnPos + new Vector2Int(x, y) - room.GridPositionsPerIndex[index];
                Vector4 connections = room.connections[i];

                for (int j = 0; j < 4; j++) {
                    Vector2Int neighborTile = roomTile + (j == 0 ? Vector2Int.right : (j == 1 ? Vector2Int.left : (j == 2 ? Vector2Int.up : Vector2Int.down)));
                    if (dungeon.TryGetValue(neighborTile, out Vector4 neighborConnections)) {
                        if ((j == 0 && Mathf.Abs(neighborConnections.y - connections.x) > 0.01f) ||
                            (j == 1 && Mathf.Abs(neighborConnections.x - connections.y) > 0.01f) ||
                            (j == 2 && Mathf.Abs(neighborConnections.w - connections.z) > 0.01f) ||
                            (j == 3 && Mathf.Abs(neighborConnections.z - connections.w) > 0.01f))
                            return false;
                    }
                }

                i++;
            }
        }

        return !Enumerable.Range(0, room.size.x)
                .SelectMany(x => Enumerable.Range(0, room.size.y)
                .Select(y => spawnPos + new Vector2Int(x, y) - room.GridPositionsPerIndex[index]))
                .Any(tile => dungeon.ContainsKey(tile));
    }
}