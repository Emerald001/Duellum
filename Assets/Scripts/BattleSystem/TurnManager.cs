using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private UnitController PlayerUnitPrefab;
    [SerializeField] private UnitController EnemyUnitPrefab;

    [SerializeField] private List<UnitData> PlayerUnitsToSpawn;
    [SerializeField] private List<UnitData> EnemyUnitsToSpawn;

    [Header("GridSettings")]
    [SerializeField] private OriginalMapGenerator GridGenerator;

    public TurnController CurrentPlayer => currentPlayer;
    public bool IsDone { get; private set; }

    private UnitFactory unitFactory;

    private List<TurnController> players = new();
    private TurnController currentPlayer;
    private int currentPlayerIndex;

    private void Awake() {
        unitFactory = new();

        GridGenerator.SetUp();

        SpawnUnits();
        NextPlayer();
    }

    private void Update() {
        if (currentPlayer == null)
            return;

        if (currentPlayer.IsDone) {
            NextPlayer();
            return;
        }

        currentPlayer.OnUpdate();
    }

    private void SpawnUnits() {
        for (int i = 0; i < GridStaticFunctions.PlayerSpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.PlayerSpawnPos[i];

            var unit = unitFactory.CreateUnit(PlayerUnitPrefab, PlayerUnitsToSpawn[i], spawnPos);
            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.PlayerUnitsInPlay.Add(unit);
        }
        players.Add(new PlayerTurnController());

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPos[i];

            var unit = unitFactory.CreateUnit(EnemyUnitPrefab, EnemyUnitsToSpawn[i], spawnPos);
            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.EnemyUnitsInPlay.Add(unit);
        }
        players.Add(new EnemyTurnController());
    }

    private void NextPlayer() {
        if (currentPlayerIndex > players.Count - 1) {
            EventManager<BattleEvents>.Invoke(BattleEvents.NewTurn);

            currentPlayerIndex = 0;
        }

        currentPlayer?.OnExit();
        currentPlayer = players[currentPlayerIndex];
        currentPlayer.OnEnter();

        currentPlayerIndex++;
    }
}

public class UnitFactory {
    public UnitController CreateUnit(UnitController prefab, UnitData data, Vector2Int spawnPos) {
        UnitController unit = Object.Instantiate(prefab);

        unit.transform.position = GridStaticFunctions.CalcSquareWorldPos(spawnPos);

        unit.SetUp(data, spawnPos);
        return unit;
    }
}

public enum BattleEvents {
    SetupBattle,
    StartBattle,
    EndUnitTurn,
    NewTurn,
    UnitDeath,
    GrabbedAbilityCard,
    ReleasedAbilityCard,
    BattleEnd,
}
