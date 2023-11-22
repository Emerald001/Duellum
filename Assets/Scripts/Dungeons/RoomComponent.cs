using System.Collections.Generic;
using UnityEngine;

public class RoomComponent : MonoBehaviour {
    public List<Vector2Int> gridPositions;
    public List<Hex> gridValues;
    public List<float> gridHeights;

    public void Editor_SetUp(List<Vector2Int> gridPositions, List<Hex> gridValues, List<float> gridHeights) {
        this.gridPositions = gridPositions;
        this.gridValues = gridValues;
        this.gridHeights = gridHeights;
    }

    public void SetUp(Vector2Int offset) {
        for (int i = 0; i < gridValues.Count; i++) {
            gridValues[i].GridPos = gridPositions[i];
            gridValues[i].StandardWorldPosition = gridValues[i].transform.position;
        }
    }
}
