using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "AllUnitSelector", menuName = "Selectors/AllUnitSelector")]
public class AllUnitSelector : BaseSelector {
    public override List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int range, int i2) {
        List<Vector2Int> result = new();

        // Get all Units

        return result;
    }
}