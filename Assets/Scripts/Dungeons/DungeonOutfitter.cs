using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonOutfitter : MonoBehaviour {
    [SerializeField] private GameObject LinePrefab;
    [SerializeField] private EnemyBehaviour EnemyPrefab;
    [SerializeField] private PlayerController PlayerPrefab;

    [SerializeField] private DungeonChest ChestPrefab;

    private List<RoomComponent> allRooms = new();
    private List<RoomComponent> mainRoomSequence = new();
    private List<List<RoomComponent>> roomsInProgression = new();

    private PlayerController playerController;

    public void OutfitDungeon() {
        foreach (var room in GridStaticFunctions.Dungeon) {
            Vector2Int gridpos = room.Key;
            RoomComponent roomComp = room.Value;

            if (gridpos == roomComp.indexZeroGridPos) {
                OverwriteGridPositions(roomComp);
                roomComp.SetUp(gridpos * GridStaticFunctions.TilesPerRoom);
            }
        }

        CreatePathThroughDungeon();

        SpawnPlayer();
        SpawnChests();
        SpawnEnemies();

        EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, "Dungeon Generation Finished");
    }

    private void OverwriteGridPositions(RoomComponent room) {
        switch (room.rotationIndex) {
            case 1:
                int index1 = 0;
                for (int x = 0; x < (room.size.x * GridStaticFunctions.TilesPerRoom); x++) {
                    for (int y = room.size.y * GridStaticFunctions.TilesPerRoom - 1; y >= 0; y--) {
                        room.gridPositions[index1] = new(x, y);
                        index1++;
                    }
                }
                break;
            case 2:
                int index2 = 0;
                for (int y = (room.size.y * GridStaticFunctions.TilesPerRoom) - 1; y >= 0; y--) {
                    for (int x = (room.size.x * GridStaticFunctions.TilesPerRoom) - 1; x >= 0; x--) {
                        room.gridPositions[index2] = new(x, y);
                        index2++;
                    }
                }
                break;
            case 3:
                int index3 = 0;
                for (int x = room.size.x * GridStaticFunctions.TilesPerRoom - 1; x >= 0; x--) {
                    for (int y = 0; y < room.size.y * GridStaticFunctions.TilesPerRoom; y++) {
                        room.gridPositions[index3] = new(x, y);
                        index3++;
                    }
                }
                break;

            default:
                return;
        }
    }

    private void CreatePathThroughDungeon() {
        foreach (var room in GridStaticFunctions.Dungeon) {
            Vector2Int gridpos = room.Key;
            RoomComponent roomComp = room.Value;

            if (gridpos == roomComp.indexZeroGridPos)
                allRooms.Add(roomComp);
        }
        RoomComponent bossroom = allRooms.First(x => x.gridValues.Select(x => x.Type).Contains(TileType.BossSpawn));

        float largestDis = 0;
        RoomComponent bestStartRoom = null;
        List<RoomComponent> endRooms = new();
        foreach (RoomComponent room in allRooms) {
            int counter = 0;
            foreach (var item in room.connections) {
                if (item.x != GridStaticFunctions.CONST_INT ||
                    item.y != GridStaticFunctions.CONST_INT ||
                    item.z != GridStaticFunctions.CONST_INT ||
                    item.w != GridStaticFunctions.CONST_INT)
                    counter++;
            }

            if (counter > 1)
                continue;

            IEnumerable<TileType> values = room.gridValues.Select(x => x.Type);
            if (values.Contains(TileType.Spawn)) {
                float distance = Vector3.Distance(bossroom.transform.position, room.transform.position);
                if (distance > largestDis) {
                    largestDis = distance;
                    bestStartRoom = room;
                }
            }

            if (values.Contains(TileType.Special) || values.Contains(TileType.Spawn))
                endRooms.Add(room);
        }
        Dictionary<RoomComponent, RoomComponent> parentDictionary = new();

        List<RoomComponent> openSet = new();
        List<RoomComponent> closedSet = new();
        openSet.Add(bestStartRoom);

        void AddRooms(Vector2Int newRoomPos, RoomComponent parentRoom) {
            RoomComponent room = GridStaticFunctions.Dungeon[newRoomPos];

            if (openSet.Contains(room) || closedSet.Contains(room))
                return;

            openSet.Add(room);
            parentDictionary.Add(room, parentRoom);
        }

        while (openSet.Count > 0) {
            RoomComponent currentRoom = openSet[0];

            int index = 0;
            for (int x = 0; x < currentRoom.size.x; x++) {
                for (int y = 0; y < currentRoom.size.y; y++) {
                    Vector2Int tile = currentRoom.indexZeroGridPos + new Vector2Int(x, y);
                    Vector4 connections = currentRoom.connections[index];

                    if (connections.x != GridStaticFunctions.CONST_INT)
                        AddRooms(tile + new Vector2Int(1, 0), currentRoom);
                    if (connections.y != GridStaticFunctions.CONST_INT)
                        AddRooms(tile + new Vector2Int(-1, 0), currentRoom);
                    if (connections.z != GridStaticFunctions.CONST_INT)
                        AddRooms(tile + new Vector2Int(0, 1), currentRoom);
                    if (connections.w != GridStaticFunctions.CONST_INT)
                        AddRooms(tile + new Vector2Int(0, -1), currentRoom);

                    index++;
                }
            }

            closedSet.Add(currentRoom);
            openSet.RemoveAt(0);
        }

        SetMainSequence(parentDictionary, bestStartRoom, bossroom);
        DefineSidePaths(endRooms, parentDictionary);
    }

    private void SetMainSequence(Dictionary<RoomComponent, RoomComponent> parentDictionary, RoomComponent startRoom, RoomComponent endRoom) {
        List<RoomComponent> path = new();
        List<RoomComponent> checkedRooms = new();

        RoomComponent currentRoom = endRoom;
        while (currentRoom != startRoom) {
            path.Add(currentRoom);
            currentRoom = parentDictionary[currentRoom];
        }

        path.Add(currentRoom);
        path.Reverse();

        LineRenderer line = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
        line.material.color = new(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++) {
            line.SetPosition(i, CalculateWorldPosition(path[i]));
            checkedRooms.Add(path[i]);
            mainRoomSequence.Add(path[i]);
        }

        roomsInProgression.Add(new(mainRoomSequence));
    }

    private void DefineSidePaths(List<RoomComponent> endRooms, Dictionary<RoomComponent, RoomComponent> parentDictionary) {
        List<RoomComponent> checkedRooms = new(mainRoomSequence);

        foreach (var room in endRooms) {
            List<RoomComponent> smallPath = new();

            LineRenderer line = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
            line.material.color = new(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            line.positionCount = 1;

            RoomComponent currentRoom = room;
            line.SetPosition(0, CalculateWorldPosition(currentRoom));

            int index = 1;
            while (!checkedRooms.Contains(currentRoom)) {
                line.positionCount++;
                smallPath.Add(currentRoom);

                checkedRooms.Add(currentRoom);
                currentRoom = parentDictionary[currentRoom];
                line.SetPosition(index, CalculateWorldPosition(currentRoom));

                index++;
                if (index > 20)
                    break;
            }

            //RoomComponent connectingRoom = currentRoom;
            //RoomComponent nextMainSequenceRoom = parentDictionary[connectingRoom];

            //Vector3 doorPosition = new();
            //foreach (var item in connectingRoom.connections) {
            //    if (item.x == GridStaticFunctions.CONST_INT &&
            //        item.y == GridStaticFunctions.CONST_INT &&
            //        item.z == GridStaticFunctions.CONST_INT &&
            //        item.w == GridStaticFunctions.CONST_INT)
            //        continue;

            //    int i = connectingRoom.connections.IndexOf(item);

            //    Vector2 direction = new Vector2(item.x + item.y, item.z + item.w).normalized;
            //    Vector2Int localGridPos = connectingRoom.gridPositionsPerIndex[i];
            //    Vector2Int newPos = connectingRoom.indexZeroGridPos + localGridPos + new Vector2Int((int)direction.x, (int)direction.y);

            //    if (GridStaticFunctions.Dungeon[newPos] == nextMainSequenceRoom) {
            //        Vector3 worldPos = GridStaticFunctions.CalcWorldPos((connectingRoom.indexZeroGridPos + localGridPos) * GridStaticFunctions.TilesPerRoom);
            //        Vector2 dir = direction * 5.5f;

            //        //float height = room.gridHeights;
            //        //doorPosition = worldPos + new Vector3(dir.x, height, dir.y);

            //        break;
            //    }
            //}

            roomsInProgression.Add(new(smallPath));
        }
    }

    private void SpawnChests() {
        foreach (var room in allRooms) {
            foreach (var position in room.gridValues.Where(x => x.Type == TileType.Special)) {
                Vector2Int spawnGridPosition = GridStaticFunctions.GetGridPosFromTileGameObject(position.gameObject);
                Vector3 worldPosition = GridStaticFunctions.CalcWorldPos(spawnGridPosition);

                DungeonChest chest = Instantiate(ChestPrefab);
                chest.Player = playerController.transform;
                chest.transform.position = worldPosition;
                chest.transform.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            }
        }
    }

    private void SpawnEnemies() {
        foreach (RoomComponent room in allRooms) {
            int enemyIndex = 0;
            foreach (var position in room.gridValues.Where(x => x.Type == TileType.EnemySpawn)) {
                int index = room.gridValues.IndexOf(position);
                Vector2Int spawnGridPosition = GridStaticFunctions.GetGridPosFromTileGameObject(position.gameObject);
                Vector3 worldPosition = GridStaticFunctions.CalcWorldPos(spawnGridPosition);

                EnemyBehaviour enemy = Instantiate(EnemyPrefab);
                enemy.Setup(playerController.transform, room.EnemyTeams[enemyIndex].Enemies, spawnGridPosition);

                enemy.transform.position = worldPosition;
                enemyIndex++;
            }
        }
    }

    private void SpawnPlayer() {
        RoomComponent spawnRoom = mainRoomSequence[0];

        Vector2Int spawnGridPosition = GridStaticFunctions.GetGridPosFromTileGameObject(spawnRoom.gridValues.First(x => x.Type == TileType.Spawn).gameObject);
        Vector3 worldPosition = GridStaticFunctions.CalcWorldPos(spawnGridPosition);

        PlayerController player = Instantiate(PlayerPrefab, worldPosition, Quaternion.identity);
        player.SetUp(spawnGridPosition);
        playerController = player;

        EventManager<CameraEventType, float>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, 30);
        EventManager<CameraEventType, Transform>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, player.transform);
    }

    private Vector3 CalculateWorldPosition(RoomComponent room) {
        Vector3 worldPosition = new();
        for (int x = 0; x < room.size.x; x++) {
            for (int y = 0; y < room.size.y; y++)
                worldPosition += new Vector3((room.indexZeroGridPos.x + x) * GridStaticFunctions.TilesPerRoom, 5, (room.indexZeroGridPos.y + y) * GridStaticFunctions.TilesPerRoom);
        }

        return worldPosition / (room.size.x * room.size.y);
    }
}
