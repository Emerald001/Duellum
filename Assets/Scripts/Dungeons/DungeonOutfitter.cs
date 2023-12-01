using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonOutfitter : MonoBehaviour {
    [SerializeField] private LineRenderer lineRenderer;

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
        // ordering the rooms
        List<Tuple<int, RoomComponent>> rooms = new();
        foreach (KeyValuePair<Vector2Int, RoomComponent> room in GridStaticFunctions.Dungeon) {
            Vector2Int gridPosition = room.Value.indexZeroGridPos;

            if (room.Value.size.x > 0 || room.Value.size.y > 0)
                continue;

            int amount = gridPosition.x + gridPosition.y;
            rooms.Add(new(amount, room.Value));
        }
        rooms = rooms.OrderBy(obj => obj.Item1).ToList();

        // Get next rooms
        Dictionary<Vector2Int, Vector2Int> parentDictionary = new();

        List<Vector2Int> openList = new();
        List<Vector2Int> closedList = new();

        openList.Add(rooms[0].Item2.indexZeroGridPos);
        Vector2Int currentPosition = openList[0];

        void AddRooms(Vector2Int newRoomPos, Vector2Int parentPos) {
            RoomComponent room = GridStaticFunctions.Dungeon[newRoomPos];
            openList.Add(newRoomPos);

            // Do a manual Ripple instead!
            GridStaticFunctions.RippleThroughGridPositions(newRoomPos, 20, (gridPos, i) => {
                if (GridStaticFunctions.Dungeon[newRoomPos] != room)
                    return;

                parentDictionary.Add(currentPosition, gridpos);
            }, false);

            //for (int x = 0; x < room.size.x; x++) {
            //    for (int y = 0; y < room.size.y; y++)
            //        openList.Add(offset + new Vector2Int(x, y));
            //}
}

        int breakout = 0;
        while (currentPosition != rooms[^1].Item2.indexZeroGridPos && breakout < 30) {
            currentPosition = openList[0];
            Vector4 connections = GridStaticFunctions.DungeonConnections[currentPosition];

            if (connections.x != GridStaticFunctions.CONST_INT) 
                AddRooms(currentPosition + new Vector2Int(1, 0), currentPosition);
            if (connections.y != GridStaticFunctions.CONST_INT) 
                AddRooms(currentPosition + new Vector2Int(-1, 0), currentPosition);
            if (connections.z != GridStaticFunctions.CONST_INT) 
                AddRooms(currentPosition + new Vector2Int(0, 1), currentPosition);
            if (connections.w != GridStaticFunctions.CONST_INT) 
                AddRooms(currentPosition + new Vector2Int(0, -1), currentPosition);

            closedList.Add(openList[0]);
            openList.RemoveAt(0);

            // We add all the positions in it, based on its IndexZeroGridPos and their size. 
            // All found positions we add to the open set.
            // We loop through the open set to with the connections from the dungeon generator.
            // Once we find a room with multiple tiles, we add all the tiles with their grid positions linked to the last. 

            breakout++;
        }

        Debug.Log($"Breakout at {breakout}");

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, new(rooms[0].Item2.indexZeroGridPos.x * 13, 20, rooms[0].Item2.indexZeroGridPos.y * 13));
        lineRenderer.SetPosition(1, new(rooms[^1].Item2.indexZeroGridPos.x * 13, 20, rooms[^1].Item2.indexZeroGridPos.y * 13));
    }
}
