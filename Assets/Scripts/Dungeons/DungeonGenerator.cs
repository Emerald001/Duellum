using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour {
    [SerializeField] private int roomAmount;
    [SerializeField] private int bigRoomAmount;
    [SerializeField] [Range(0, 100)] private int bigRoomPercentage;

    [SerializeField] private List<DungeonRoomSO> bigRoomList;
    [SerializeField] private List<DungeonRoomSO> endRoomList;

    [SerializeField] private bool generateInstantly;
    [SerializeField] private float generationSpeed;

    [SerializeField] private DungeonOutfitter outfitter;

    private int tileSize => GridStaticFunctions.TilesPerRoom;

    private readonly Dictionary<Vector2Int, Vector4> dungeonConnections = new();
    private readonly List<int> usedIds = new();
    private int spawnedBigRoomAmount;

    private Transform dungeonParent;

    private void OnEnable() {
        EventManager<DungeonEvents>.Subscribe(DungeonEvents.StartGeneration, GenerateDungeon);
    }
    private void OnDisable() {
        EventManager<DungeonEvents>.Unsubscribe(DungeonEvents.StartGeneration, GenerateDungeon);
    }

    private void GenerateDungeon() {
        dungeonParent = new GameObject("DungeonRooms").transform;

        StartCoroutine(SpawnRooms());
    }

    private IEnumerator SpawnRooms() {
        List<Vector2Int> openSet = new() { new(0, 0) };
        Dictionary<Vector2Int, float> heights = new() { { new(0, 0), 0 } };
        Dictionary<Vector2Int, GameObject> debugObjects = new();

        void AddNewTile(Vector2Int currentPos, Vector2Int direction, DungeonRoomTile spawnedRoom, float height) {
            Vector2Int newTile = currentPos + direction;
            if (dungeonConnections.ContainsKey(newTile) || openSet.Contains(newTile))
                return;

            openSet.Add(newTile);
            heights.Add(newTile, spawnedRoom.Height + height);

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
            Tuple<DungeonRoomTile, RoomComponent> roomData = SpawnRoom(new(bigRoomList), currentPos, heights[currentPos]);
            roomsSpawned++;

            if (!generateInstantly) {
                if (debugObjects.ContainsKey(currentPos))
                    debugObjects[currentPos].GetComponent<Renderer>().material.color = Color.yellow;
                yield return new WaitForSeconds(generationSpeed);
            }

            Vector2Int offset = Vector2Int.zero - roomData.Item1.GridPositionsPerIndex[roomData.Item1.CurrentIndex];
            int index = 0;
            for (int x = 0; x < roomData.Item1.size.x; x++) {
                for (int y = 0; y < roomData.Item1.size.y; y++) {
                    Vector2Int tile = currentPos + new Vector2Int(x, y) + offset;
                    Vector4 connections = roomData.Item1.connections[index];

                    if (x == 0 && y == 0)
                        roomData.Item2.indexZeroGridPos = tile;

                    if (connections.x != GridStaticFunctions.CONST_INT)
                        AddNewTile(tile, new Vector2Int(1, 0), roomData.Item1, connections.x);
                    if (connections.y != GridStaticFunctions.CONST_INT)
                        AddNewTile(tile, new Vector2Int(-1, 0), roomData.Item1, connections.y);
                    if (connections.z != GridStaticFunctions.CONST_INT)
                        AddNewTile(tile, new Vector2Int(0, 1), roomData.Item1, connections.z);
                    if (connections.w != GridStaticFunctions.CONST_INT)
                        AddNewTile(tile, new Vector2Int(0, -1), roomData.Item1, connections.w);

                    if (openSet.Contains(tile))
                        openSet.Remove(tile);

                    if (!generateInstantly) {
                        if (debugObjects.ContainsKey(tile))
                            debugObjects[tile].GetComponent<Renderer>().material.color = Color.red;
                        //yield return new WaitForSeconds(generationSpeed);
                    }

                    dungeonConnections.Add(tile, connections);
                    GridStaticFunctions.Dungeon.Add(tile, roomData.Item2);
                    index++;
                }
            }
        }

        foreach (var currentPos in openSet) {
            Tuple<DungeonRoomTile, RoomComponent> roomData = SpawnRoom(new(endRoomList), currentPos, heights[currentPos], true);

            if (!generateInstantly) {
                if (debugObjects.ContainsKey(currentPos))
                    debugObjects[currentPos].GetComponent<Renderer>().material.color = Color.yellow;
                yield return new WaitForSeconds(generationSpeed);
            }

            Vector2Int offset = Vector2Int.zero - roomData.Item1.GridPositionsPerIndex[roomData.Item1.CurrentIndex];
            int index = 0;
            for (int x = 0; x < roomData.Item1.size.x; x++) {
                for (int y = 0; y < roomData.Item1.size.y; y++) {
                    Vector2Int tile = currentPos + new Vector2Int(x, y) + offset;
                    dungeonConnections.Add(tile, roomData.Item1.connections[index]);

                    if (x == 0 && y == 0)
                        roomData.Item2.indexZeroGridPos = tile;

                    if (!generateInstantly) {
                        if (debugObjects.ContainsKey(tile))
                            debugObjects[tile].GetComponent<Renderer>().material.color = Color.red;
                        yield return new WaitForSeconds(generationSpeed);
                    }

                    GridStaticFunctions.Dungeon.Add(tile, roomData.Item2);
                    index++;
                }
            }
        }

        if (!IsDungeonValid(roomsSpawned)) {
            Debug.Log("Dungeon invalid... Regenerating...");

            GridStaticFunctions.Dungeon.Clear();
            dungeonConnections.Clear();
            usedIds.Clear();
            spawnedBigRoomAmount = 0;

            Destroy(dungeonParent.gameObject);

            yield return null;

            GenerateDungeon();
            yield break;
        }

        openSet.Clear();
        GridStaticFunctions.DungeonConnections = new(dungeonConnections);

        outfitter.OutfitDungeon();
    }

    private Tuple<DungeonRoomTile, RoomComponent> SpawnRoom(List<DungeonRoomSO> listToUse, Vector2Int position, float height, bool spawnFirst = false) {
        var room = GetRoom(listToUse, position, spawnFirst);
        if (room.Id != 0)
            usedIds.Add(room.Id);

        Vector4 connection = room.connections[room.CurrentIndex];
        if (connection.x != GridStaticFunctions.CONST_INT && dungeonConnections.ContainsKey(position + new Vector2Int(1, 0)))
            room.Height = height - connection.x;
        else if (connection.y != GridStaticFunctions.CONST_INT && dungeonConnections.ContainsKey(position + new Vector2Int(-1, 0)))
            room.Height = height - connection.y;
        else if (connection.z != GridStaticFunctions.CONST_INT && dungeonConnections.ContainsKey(position + new Vector2Int(0, 1)))
            room.Height = height - connection.z;
        else if (connection.w != GridStaticFunctions.CONST_INT && dungeonConnections.ContainsKey(position + new Vector2Int(0, -1)))
            room.Height = height - connection.w;

        RoomComponent spawnedRoom = Instantiate(room.prefab, dungeonParent);
        spawnedRoom.name = room.name;
        spawnedRoom.transform.position = CalculateWorldPosition(position, room.Height, room);
        spawnedRoom.transform.eulerAngles = new(0, room.RotationIndex * 90, 0);
        spawnedRoom.size = room.size;
        spawnedRoom.rotationIndex = room.RotationIndex;

        spawnedRoom.gridPositionsPerIndex = new(room.GridPositionsPerIndex);
        spawnedRoom.connections = new(room.connections);

        spawnedRoom.gridPositionsPerIndex = new(room.GridPositionsPerIndex);
        spawnedRoom.connections = new(room.connections);

        return new(room, spawnedRoom);
    }

    private DungeonRoomTile GetRoom(List<DungeonRoomSO> listToUse, Vector2Int position, bool spawnFirst) {
        int counter = 1;
        List<DungeonRoomTile> availableTiles = listToUse.Select(x => x.room).ToList()
        .SelectMany(item => Enumerable.Range(0, (item.size.x > 1 || item.size.y > 1) ? 1 : 4)
            .Select(i => {
                DungeonRoomTile roomTile = new(item.name, item.size, item.prefab, item.connections, counter);
                roomTile.Init(0);
                counter++;

                return roomTile;
            }))
        .Where(tile => {
            if (spawnedBigRoomAmount >= bigRoomAmount && (tile.size.x > 1 || tile.size.y > 1))
                return false;

            if (usedIds.Contains(tile.Id))
                return false;

            List<int> availableIndices = tile.GridPositionsPerIndex.Keys
                .Where(r => CheckForFit(tile, position, r))
                .ToList();

            if (availableIndices.Count > 0) {
                tile.CurrentIndex = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                return true;
            }

            return false;
        })
        .ToList();

        List<DungeonRoomTile> availableSmallTiles = endRoomList.Select(x => x.room).ToList()
        .SelectMany(item => Enumerable.Range(0, 4)
            .Select(i => {
                DungeonRoomTile roomTile = new(item.name, item.size, item.prefab, item.connections, item.Id);
                roomTile.Init(i);

                return roomTile;
            }))
        .Where(tile => {
            if (usedIds.Contains(tile.Id))
                return false;

            List<int> availableIndices = tile.GridPositionsPerIndex.Keys
                .Where(r => CheckForFit(tile, position, r))
                .ToList();

            if (availableIndices.Count > 0) {
                tile.CurrentIndex = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
                return true;
            }

            return false;
        })
        .ToList();

        List<DungeonRoomTile> roomlist = null;
        if (availableTiles.Count < 1)
            roomlist = availableSmallTiles;
        else
            roomlist = UnityEngine.Random.Range(0, 100) > bigRoomPercentage ? availableSmallTiles : availableTiles;

        if (spawnFirst)
            roomlist = availableSmallTiles;

        int index = spawnFirst ? 0 : Mathf.RoundToInt(UnityEngine.Random.Range(0, roomlist.Count));
        DungeonRoomTile room = roomlist[index];
        spawnedBigRoomAmount += (room.size.x > 1 || room.size.y > 1) ? 1 : 0;

        return room;
    }

    private Vector3 CalculateWorldPosition(Vector2Int position, float height, DungeonRoomTile room) {
        Vector2Int offset = Vector2Int.zero - room.GridPositionsPerIndex[room.CurrentIndex];

        Vector3 worldPosition = new();
        for (int x = 0; x < room.size.x; x++) {
            for (int y = 0; y < room.size.y; y++)
                worldPosition += new Vector3((position.x + x + offset.x) * tileSize, height, (position.y + y + offset.y) * tileSize);
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
                    if (dungeonConnections.TryGetValue(neighborTile, out Vector4 neighborConnections)) {
                        if ((j == 0 && ((neighborConnections.y != GridStaticFunctions.CONST_INT) ^ (connections.x != GridStaticFunctions.CONST_INT))) ||
                            (j == 1 && ((neighborConnections.x != GridStaticFunctions.CONST_INT) ^ (connections.y != GridStaticFunctions.CONST_INT))) ||
                            (j == 2 && ((neighborConnections.w != GridStaticFunctions.CONST_INT) ^ (connections.z != GridStaticFunctions.CONST_INT))) ||
                            (j == 3 && ((neighborConnections.z != GridStaticFunctions.CONST_INT) ^ (connections.w != GridStaticFunctions.CONST_INT))))
                            return false;
                    }
                }

                i++;
            }
        }

        return !Enumerable.Range(0, room.size.x)
                .SelectMany(x => Enumerable.Range(0, room.size.y)
                .Select(y => spawnPos + new Vector2Int(x, y) - room.GridPositionsPerIndex[index]))
                .Any(tile => dungeonConnections.ContainsKey(tile));
    }

    private bool IsDungeonValid(int roomAmount) {
        if (roomAmount < this.roomAmount / 2)
            return false;

        List<RoomComponent> endRooms = new();
        foreach (KeyValuePair<Vector2Int, RoomComponent> room in GridStaticFunctions.Dungeon) {
            int counter = 0;
            foreach (var item in room.Value.connections) {
                if (item.x != GridStaticFunctions.CONST_INT ||
                    item.y != GridStaticFunctions.CONST_INT ||
                    item.z != GridStaticFunctions.CONST_INT ||
                    item.w != GridStaticFunctions.CONST_INT)
                    counter++;
            }

            if (counter > 1)
                continue;

            Vector2Int gridPosition = room.Value.indexZeroGridPos;
            int amount = gridPosition.x + gridPosition.y;
            endRooms.Add(room.Value);
        }

        int spawnPointAmounts = 0;
        foreach (RoomComponent room in endRooms) {
            IEnumerable<TileType> values = room.gridValues.Select(x => x.Type);
            if (values.Contains(TileType.Spawn))
                spawnPointAmounts++;
        }

        if (endRooms.Count() < 2 || spawnPointAmounts < 1)
            return false;

        List<RoomComponent> rooms = new();
        foreach (var room in GridStaticFunctions.Dungeon) {
            Vector2Int gridpos = room.Key;
            RoomComponent roomComp = room.Value;

            if (gridpos == roomComp.indexZeroGridPos)
                rooms.Add(roomComp);
        }

        int bossCount = 0;
        int enemyCount = 0;
        foreach (RoomComponent room in rooms) {
            var tmp = room.gridValues.Select(x => x.Type);

            if (tmp.Contains(TileType.BossSpawn)) {
                enemyCount++;
                bossCount++;
            }

            if (tmp.Contains(TileType.EnemySpawn))
                enemyCount++;
        }

        if (bossCount < 1 || bossCount > 1 || enemyCount < 3)
            return false;

        return true;
    }
}

public enum DungeonEvents {
    StartGeneration,
    GenerationDone,
}