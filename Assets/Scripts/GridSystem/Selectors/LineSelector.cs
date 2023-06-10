using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "LineSelector", menuName = "Selectors/LineSelector")]
public class LineSelector : BaseSelector {
    public override List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int range, int index) {
        List<Vector2Int> result = new() {
            startPos
        };

        if (index > 6) {
            Debug.LogError("Index is above 6, this is not possible!");
            return null;
        }

        if (index == 6)
            for (int j = 0; j < 6; j++) {
                result.Add(startPos);

                for (int i = 0; i < range; i++) {
                    if (GridStaticFunctions.TryGetHexNeighbour(result[^1], j, out Vector2Int pos))
                        result.Add(pos);
                }

                result.Remove(startPos);
            }
        else
            for (int i = 0; i < range; i++) {
                if (GridStaticFunctions.TryGetHexNeighbour(result[^1], index, out Vector2Int pos))
                    result.Add(pos);
            }
        result.Remove(startPos);

        return result;
    }
}