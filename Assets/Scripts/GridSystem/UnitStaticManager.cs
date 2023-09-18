using System.Collections.Generic;
using UnityEngine;

public static class UnitStaticManager {
    public static Dictionary<UnitController, Vector2Int> UnitPositions { get; set; } = new();

    public static List<UnitController> LivingUnitsInPlay { get; set; } = new();

    public static List<UnitController> DeadUnitsInPlay { get; set; } = new();
    public static List<UnitController> UnitsWithTurnLeft { get; set; } = new();

    public static List<UnitController> EnemyUnitsInPlay { get; set; } = new();
    public static List<UnitController> PlayerUnitsInPlay { get; set; } = new();

    public static void SetUnitPosition(UnitController unit, Vector2Int pos) {
        UnitPositions[unit] = pos;
    }

    public static Vector2Int GetUnitPosition(UnitController unit) {
        return UnitPositions[unit];
    }

    public static bool TryGetUnitFromGridPos(Vector2Int position, out UnitController unit) {
        foreach (var item in UnitPositions) {
            if (item.Value == position) {
                unit = item.Key;
                return true;
            }
        }

        unit = null;
        return false;
    }
}