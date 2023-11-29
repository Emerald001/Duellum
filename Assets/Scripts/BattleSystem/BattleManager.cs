using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {
    [SerializeField] private UnitController PlayerUnitPrefab;
    [SerializeField] private UnitController EnemyUnitPrefab;

    public TurnController CurrentPlayer => currentPlayer;
    public bool IsDone { get; private set; }

    private UnitFactory unitFactory;

    private readonly List<TurnController> players = new();
    private TurnController currentPlayer;
    private int currentPlayerIndex;

    private Transform unitHolder;

    private void Awake() {
        unitFactory = new();
    }

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartBattle, StartBattle);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, CleanupAfterBattle);
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartBattle, StartBattle);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, CleanupAfterBattle);
    }

    private void StartBattle(BattleData data) {
        SetBattlefield(data.PlayerPos, data.EnemyPos);
        SpawnUnits(data.PlayerUnits, data.EnemyUnits);

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

    private void SetBattlefield(Vector2Int playerPos, Vector2Int enemyPos) {
        Vector2Int difference = playerPos - enemyPos;
        Vector2Int middlePoint = playerPos + difference / 2;

        List<Vector2Int> points = new();
        for (int x = -((BattleMapSize - 1) / 2); x <= ((BattleMapSize - 1) / 2); x++) {
            for (int y = -((BattleMapSize - 1) / 2); y <= ((BattleMapSize - 1) / 2); y++)
                points.Add(middlePoint + new Vector2Int(x, y));
        }

        GridStaticFunctions.PlayerSpawnPos.Add(middlePoint + new Vector2Int(-((BattleMapSize - 1) / 2), -1));
        GridStaticFunctions.PlayerSpawnPos.Add(middlePoint + new Vector2Int(-((BattleMapSize - 1) / 2), 0));
        GridStaticFunctions.PlayerSpawnPos.Add(middlePoint + new Vector2Int(-((BattleMapSize - 1) / 2), 1));

        GridStaticFunctions.EnemySpawnPos.Add(middlePoint + new Vector2Int(((BattleMapSize - 1) / 2), -1));
        GridStaticFunctions.EnemySpawnPos.Add(middlePoint + new Vector2Int(((BattleMapSize - 1) / 2), 0));
        GridStaticFunctions.EnemySpawnPos.Add(middlePoint + new Vector2Int(((BattleMapSize - 1) / 2), 1));

        GridStaticFunctions.SetBattleGrid(points);
    }

    private void SpawnUnits(List<UnitData> playerUnitsToSpawn, List<UnitData> enemyUnitsToSpawn) {
        unitHolder = new GameObject("UnitHolder").transform;

        for (int i = 0; i < GridStaticFunctions.PlayerSpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.PlayerSpawnPos[i];

            UnitController unit = unitFactory.CreateUnit(playerUnitsToSpawn[i], spawnPos, PlayerUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(new(1, 0));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.PlayerUnitsInPlay.Add(unit);
        }
        players.Add(new PlayerTurnController());

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPos[i];

            UnitController unit = unitFactory.CreateUnit(enemyUnitsToSpawn[i], spawnPos, EnemyUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(new(-1, 0));

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

    private void CleanupAfterBattle() {
        currentPlayer = null;
        players.Clear();
        currentPlayerIndex = 0;

        Destroy(unitHolder.gameObject);
        GridStaticFunctions.ResetAllTileColors();

        UnitStaticManager.Reset();
    }

    private int BattleMapSize => GridStaticFunctions.BattleMapSize;
}

public enum BattleEvents {
    SetupBattle,
    StartBattle,
    EndUnitTurn,
    NewTurn,
    UnitHit,
    UnitDeath,
    UnitRevive,
    GiveAbilityCard,
    GiveCard,
    SpawnAbilityCard,
    GrabbedAbilityCard,
    ReleasedAbilityCard,
    BattleEnd,
}
