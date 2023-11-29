using UnityEngine;

public class UnitFactory {
    public UnitController CreateUnit(UnitData data, Vector2Int spawnPos, UnitController prefab, Transform parent) {
        UnitController unit = Object.Instantiate(prefab, parent);

        unit.transform.position = GridStaticFunctions.CalcWorldPos(spawnPos);
        unit.SetUp(data, spawnPos);

        return unit;
    }
}
