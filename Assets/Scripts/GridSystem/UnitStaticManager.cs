using System.Collections.Generic;
using UnityEngine;

public static class UnitStaticManager {
    // Needs to be reset with Shift + R
    public static List<UnitData> PlayerPickedUnits { get; set; } = new();
    public static List<EnemyBehaviour> SpawnedEnemies { get; set; } = new();

    public static Dictionary<UnitController, Vector2Int> UnitPositions { get; set; } = new();
    public static Dictionary<int, List<UnitController>> UnitTeams { get; set; } = new();

    public static List<UnitController> LivingUnitsInPlay { get; set; } = new();
    public static List<UnitController> DeadUnitsInPlay { get; set; } = new();
    public static List<UnitController> UnitsWithTurnLeft { get; set; } = new();
    
    public static void Reset() {
        UnitPositions.Clear();
        LivingUnitsInPlay.Clear();
        DeadUnitsInPlay.Clear();
        UnitsWithTurnLeft.Clear();
        UnitTeams.Clear();
    }

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

    public static void UnitDeath(UnitController unit) {
        var deathPosition = GetUnitPosition(unit);
        LivingUnitsInPlay.Remove(unit);
        DeadUnitsInPlay.Add(unit);
        int id = unit.OwnerID;

        if (UnitsWithTurnLeft.Contains(unit))
            UnitsWithTurnLeft.Remove(unit);

        UnitTeams[id].Remove(unit);
        if (UnitTeams[id].Count < 1) {
            EventManager<BattleEvents, Vector2Int>.Invoke(BattleEvents.BattleEnd, deathPosition);
            EventManager<BattleEvents, int>.Invoke(BattleEvents.BattleEnd, id);
            EventManager<BattleEvents>.Invoke(BattleEvents.BattleEnd);
        }

        EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitDeath, unit);
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_successAttack");
    }

    public static void RemoveUnitFromOrder(UnitController unit) {
        if (UnitsWithTurnLeft.Contains(unit))
            UnitsWithTurnLeft.Remove(unit);
    }

    public static void ReviveUnit(UnitController unit, int index) {
        DeadUnitsInPlay.Remove(unit);
        UnitTeams[index].Add(unit);
        LivingUnitsInPlay.Add(unit);
    }

    public static void MoveUnit(int moveToID, UnitController unit) {
        UnitTeams[unit.OwnerID].Remove(unit);
        UnitTeams[moveToID].Add(unit);
    }

    public static List<UnitController> GetEnemies(int id) {
        List<UnitController> result = new();

        foreach (var item in UnitTeams) {
            if (item.Key == id)
                continue;

            result.AddRange(item.Value);
        }

        return result;
    }
}
