using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "FriendlyUnitSelector", menuName = "Selectors/FriendlyUnitSelector")]
public class FriendlyUnitSelector : BaseSelector {
    public override List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int range, int i2) {
        List<Vector2Int> result = new();

        // Get all Friendly Units

        return result;
    }
}
