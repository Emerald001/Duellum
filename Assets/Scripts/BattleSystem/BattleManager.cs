using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {
    public TurnController CurrentPlayer => currentPlayer;
    public bool IsDone { get; private set; }

    private UnitFactory unitFactory;

    private List<TurnController> players = new();
    private TurnController currentPlayer;
    private int currentPlayerIndex;

    private void Awake() {
        unitFactory = new();
    }

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartBattle, StartBattle);
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartBattle, StartBattle);
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

        GridStaticFunctions.PlayerSpawnPos.Add(middlePoint + new Vector2Int(-((BattleMapSize - 1) / 2), BattleMapSize / 2 - 1));
        GridStaticFunctions.PlayerSpawnPos.Add(middlePoint + new Vector2Int(-((BattleMapSize - 1) / 2), BattleMapSize / 2));
        GridStaticFunctions.PlayerSpawnPos.Add(middlePoint + new Vector2Int(-((BattleMapSize - 1) / 2), BattleMapSize / 2 + 1));

        GridStaticFunctions.EnemySpawnPos.Add(middlePoint + new Vector2Int(((BattleMapSize - 1) / 2), BattleMapSize / 2 - 1));
        GridStaticFunctions.EnemySpawnPos.Add(middlePoint + new Vector2Int(((BattleMapSize - 1) / 2), BattleMapSize / 2));
        GridStaticFunctions.EnemySpawnPos.Add(middlePoint + new Vector2Int(((BattleMapSize - 1) / 2), BattleMapSize / 2 + 1));

        GridStaticFunctions.SetBattleGrid(points);
    }

    private void SpawnUnits(List<UnitData> playerUnitsToSpawn, List<UnitData> enemyUnitsToSpawn) {
        for (int i = 0; i < GridStaticFunctions.PlayerSpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.PlayerSpawnPos[i];

            UnitController unit = unitFactory.CreateUnit(playerUnitsToSpawn[i], spawnPos, true);
            unit.ChangeUnitRotation(new(1, 0));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.PlayerUnitsInPlay.Add(unit);
        }
        players.Add(new PlayerTurnController());

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPos[i];

            UnitController unit = unitFactory.CreateUnit(enemyUnitsToSpawn[i], spawnPos, false);
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

    private int BattleMapSize => GridStaticFunctions.BattleMapSize;
}

public class UnitFactory {
    public UnitController CreateUnit(UnitData data, Vector2Int spawnPos, bool isPlayer) {
        UnitController unit = isPlayer ? new GameObject().AddComponent<PlayerUnitController>() : new GameObject().AddComponent<EnemyUnitController>();

        unit.transform.position = GridStaticFunctions.CalcDungeonTileWorldPos(spawnPos);
        unit.SetUp(data, spawnPos);

        return unit;
    }
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

[Serializable]
public class BattleData {
    public Vector2Int PlayerPos;
    public Vector2Int EnemyPos;

    public List<UnitData> PlayerUnits;
    public List<UnitData> EnemyUnits;
}