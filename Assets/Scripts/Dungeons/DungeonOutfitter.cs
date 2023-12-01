using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DungeonOutfitter : MonoBehaviour {
    [SerializeField] private GameObject LinePrefab; 

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
        List<Tuple<int, RoomComponent>> rooms = new();
        foreach (KeyValuePair<Vector2Int, RoomComponent> room in GridStaticFunctions.Dungeon) {
            if (room.Value.size.x > 1 || room.Value.size.y > 1)
                continue;

            Vector4 connections = room.Value.connections[0];
            if (connections.x + connections.y + connections.z + connections.w > -2500)
                continue;

            Vector2Int gridPosition = room.Value.indexZeroGridPos;
            int amount = gridPosition.x + gridPosition.y;
            rooms.Add(new(amount, room.Value));
        }
        rooms = rooms.OrderBy(obj => obj.Item1).ToList();

        Dictionary<RoomComponent, RoomComponent> parentDictionary = new();

        List<RoomComponent> openSet = new();
        List<RoomComponent> closedSet = new();

        openSet.Add(rooms[0].Item2);
        RoomComponent currentRoom = openSet[0];

        void AddRooms(Vector2Int newRoomPos, RoomComponent parentRoom) {
            RoomComponent room = GridStaticFunctions.Dungeon[newRoomPos];

            if (openSet.Contains(room) || closedSet.Contains(room))
                return;

            openSet.Add(room);
            parentDictionary.Add(room, parentRoom);
        }

        while (openSet.Count > 0) {
            currentRoom = openSet[0];

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

        DrawLines(rooms.Select(x => x.Item2).ToList(), parentDictionary);
    }

    private void DrawLines(List<RoomComponent> rooms, Dictionary<RoomComponent, RoomComponent> parentDictionary) {
        List<RoomComponent> path = new();
        List<RoomComponent> checkedRooms = new();

        RoomComponent currentRoom = rooms[^1];
        while (currentRoom != rooms[0]) {
            path.Add(currentRoom);
            currentRoom = parentDictionary[currentRoom];
        }

        path.Add(currentRoom);
        path.Reverse();

        LineRenderer line = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
        line.material.color = new(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++) {
            line.SetPosition(i, CalculateWorldPosition(path[i]));
            checkedRooms.Add(path[i]);
        }

        List<RoomComponent> endCaps = rooms
            .Where(x => !path.Contains(x) && !checkedRooms.Contains(x))
            .Where(x => x.connections[0].x + x.connections[0].y + x.connections[0].z + x.connections[0].w < -2500)
            .ToList();
        foreach (var room in endCaps) {
            line = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
            line.material.color = new(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f));
            line.positionCount = 1;

            currentRoom = room;
            line.SetPosition(0, CalculateWorldPosition(currentRoom));

            int index = 1;
            while (!checkedRooms.Contains(currentRoom)) {
                line.positionCount++;

                checkedRooms.Add(currentRoom);
                currentRoom = parentDictionary[currentRoom];
                line.SetPosition(index, CalculateWorldPosition(currentRoom));

                index++;
                if (index > 20)
                    break;
            }
        }
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
