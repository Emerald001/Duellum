using UnityEngine;

public class UnitFactory {
    public UnitController CreateUnit(UnitData data, Vector2Int spawnPos, UnitController prefab) {
        UnitController unit = Object.Instantiate(prefab);

        unit.transform.position = GridStaticFunctions.CalcWorldPos(spawnPos);
        unit.SetUp(data, spawnPos);

        return unit;
    }
}
