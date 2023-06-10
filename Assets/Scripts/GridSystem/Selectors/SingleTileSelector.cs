using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "SingleTileSelector", menuName = "Selectors/SingleTileSelector")]
public class SingleTileSelector : BaseSelector {
    public override List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int range, int index) {
        List<Vector2Int> result = new() {
            startPos
        };
        return result;
    }
}