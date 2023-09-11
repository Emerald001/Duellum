using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private UnitController UnitPrefab;

    [SerializeField] private List<UnitData> PlayerUnitsToSpawn;
    [SerializeField] private List<UnitData> EnemyUnitsToSpawn;

    [Header("GridSettings")]
    [SerializeField] private OriginalMapGenerator GridGenerator;

    private List<UnitController> livingUnitsInPlay = new();
    private List<UnitController> unitAttackOrder = new();
    private UnitController currentUnit = new();

    private int CurrentTurn;

    private UnitFactory unitFactory;

    public bool IsDone { get; private set; }

    private void Awake() {
        unitFactory = new(UnitPrefab);

        GridGenerator.SetUp();

        SpawnUnits();
        NextUnit();
    }

    private void SpawnUnits() {
        foreach (var item in GridStaticFunctions.PlayerSpawnPos)
            AllUnits.Add(unitFactory.CreateUnit(PlaceholderData, item));

        foreach (var item in GridStaticFunctions.EnemySpawnPos)
            AllUnits.Add(unitFactory.CreateUnit(PlaceholderData, item));
    }

    private void OrderUnits() {

    }

    private void NextUnit() {
        currentUnit = UnitsInOrder.Dequeue();
    }
}

public class UnitFactory {
    public UnitFactory(UnitController prefab) {
        this.prefab = prefab;
    }

    private readonly UnitController prefab;
     
    public UnitController CreateUnit(UnitData data, Vector2Int spawnPos) {
        UnitController unit = Object.Instantiate(prefab);

        unit.transform.position = GridStaticFunctions.CalcSquareWorldPos(spawnPos);

        unit.SetUp(data);
        return unit;
    }
}
