using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomComponent : MonoBehaviour {
    public List<Vector2Int> gridPositions;
    public List<Hex> gridValues;

    public void SetUp(List<Vector2Int> gridPositions, List<Hex> gridValues) {
        this.gridPositions = gridPositions;
        this.gridValues = gridValues;
    }

    private void Start() {
        for (int i = 0; i < gridValues.Count; i++) {
            gridValues[i].GridPos = gridPositions[i];
            gridValues[i].StandardWorldPosition = gridValues[i].transform.position;
        }

        GridStaticFunctions.Grid = gridPositions.Zip(gridValues, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);
    }
}
