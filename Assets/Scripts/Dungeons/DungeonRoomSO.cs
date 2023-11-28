using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Rooms/New Room")]
public class DungeonRoomSO : ScriptableObject {
    public DungeonRoomTile room;
    public int id;

    public List<Vector2Int> gridPositions;
    public List<TileType> gridValues;

    public List<Vector2Int> connectionPositions;
    public List<Vector4> connectionValues;

    public List<Vector2Int> heightPositions;
    public List<float> heightValues;
}