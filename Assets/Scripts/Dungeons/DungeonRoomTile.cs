using System;
using System.Collections.Generic;
using UnityEngine;

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