using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private GameObject UnitPlaceholder;

    [Header("GridSettings")]
    [SerializeField] private OriginalMapGenerator GridGenerator;

    private void Awake() {
        GridGenerator.SetUp();

        SpawnUnits();
    }

    private void SpawnUnits() {

    }
}

public class UnitFactory {
    public GameObject CreateUnit(UnitData data) {
        return new();
    }
}