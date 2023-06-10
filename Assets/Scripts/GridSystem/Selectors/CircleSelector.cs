using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "CircleSelector", menuName = "Selectors/CircleSelector")]
public class CircleSelector : BaseSelector {
    public override List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int range, int i2) {
        List<Vector2Int> result = new();

        GridStaticFunctions.RippleThroughGridPositions(startPos, range, (currentPos, index) => {
            result.Add(currentPos);
        });

        return result;
    }
}
