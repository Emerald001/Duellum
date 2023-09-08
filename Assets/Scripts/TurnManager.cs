using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private GameObject UnitPlaceholder;
    [SerializeField] private UnitController UnitPrefab;
    [SerializeField] private UnitData PlaceholderData;

    [Header("GridSettings")]
    [SerializeField] private OriginalMapGenerator GridGenerator;

    private UnitFactory unitFactory;

    private void Awake() {
        unitFactory = new(UnitPrefab);

        GridGenerator.SetUp();

        SpawnUnits();
    }

    private void SpawnUnits() {
        foreach (var item in GridStaticFunctions.PlayerSpawnPos) {
            unitFactory.CreateUnit(PlaceholderData, item);
        }

        foreach (var item in GridStaticFunctions.EnemySpawnPos) {
            unitFactory.CreateUnit(PlaceholderData, item);
        }
    }
}

public class UnitFactory {
    public UnitFactory(UnitController prefab) {
        this.prefab = prefab;
    }

    private readonly UnitController prefab;

    public GameObject CreateUnit(UnitData data, Vector2Int spawnPos) {
        UnitController tmp = Object.Instantiate(prefab);

        tmp.transform.position = GridStaticFunctions.CalcSquareWorldPos(spawnPos); 

        return new();
    }
}
