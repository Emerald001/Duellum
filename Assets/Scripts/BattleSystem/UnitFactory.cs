using UnityEngine;

public class UnitFactory {
    public UnitController CreateUnit(int id, UnitData data, Vector2Int spawnPos, UnitController prefab, Transform parent) {
        UnitController unit = Object.Instantiate(prefab, parent);

        unit.transform.position = GridStaticFunctions.CalcWorldPos(spawnPos);
        unit.SetUp(id, data, spawnPos);

        return unit;
    }
}
