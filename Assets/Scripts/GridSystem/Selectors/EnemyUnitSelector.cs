using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "EnemyUnitSelector", menuName = "Selectors/EnemyUnitSelector")]
public class EnemyUnitSelector : BaseSelector {
    public override List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int range, int i2) {
        List<Vector2Int> result = new();

        // Get all Enemy Units

        return result;
    }
}
