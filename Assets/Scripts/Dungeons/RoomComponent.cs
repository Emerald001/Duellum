using System.Collections.Generic;
using UnityEngine;

public class RoomComponent : MonoBehaviour {
    public List<Vector2Int> gridPositions;
    public List<Hex> gridValues;
    public List<float> gridHeights;

    public Vector2Int size;
    public Vector2Int indexZeroGridPos;
    public int rotationIndex;

    public void Editor_SetUp(List<Vector2Int> gridPositions, List<Hex> gridValues, List<float> gridHeights) {
        this.gridPositions = gridPositions;
        this.gridValues = gridValues;
        this.gridHeights = gridHeights;
    }

    public void SetUp(Vector2Int offset) {
        for (int i = 0; i < gridValues.Count; i++) {
            gridValues[i].GridPos = offset + gridPositions[i];
            gridValues[i].StandardWorldPosition = gridValues[i].transform.position;

            gridValues[i].name = $"DugeonPos: {offset + gridPositions[i]} | {gridValues[i].name}";
            GridStaticFunctions.Grid.Add(offset + gridPositions[i], gridValues[i]);
        }
    }
}
