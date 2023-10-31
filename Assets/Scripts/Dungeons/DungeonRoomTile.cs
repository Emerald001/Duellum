using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonRoomTile {
    public string name;
    public Vector2Int size;
    public GameObject prefab;
    public List<Vector4> connections;

    public DungeonRoomTile(string name, Vector2Int size, GameObject prefab, List<Vector4> connections) {
        this.name = name;
        this.size = size;
        this.prefab = prefab;
        this.connections = new(connections);
    }

    public Dictionary<int, Vector2Int> GridPositionsPerIndex { get; private set; }
    public int CurrentIndex { get; set; }
    public int RotationIndex { get; set; }
    public float Height { get; set; }

    public void Init(int rotIndex) {
        if (rotIndex > 0) {
            RotationIndex = rotIndex;

            for (int i = 0; i < rotIndex; i++) {
                for (int j = 0; j < connections.Count; j++) {
                    Vector4 connection = connections[j];

                    connections[j] = new Vector4(connection.z, connection.w, connection.y, connection.x);
                }
            }
        }

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
