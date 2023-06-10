using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseSelector : ScriptableObject {
    public abstract List<Vector2Int> GetAvailableTiles(Vector2Int startPos, int i1, int i2);
}
