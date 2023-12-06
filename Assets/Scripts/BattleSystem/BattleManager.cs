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
        SetBattlefield(data);
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

    private void SetBattlefield(BattleData data) {
        List<Vector2Int> GetSpawnPositions(int unitAmount, Vector2Int startPos, Vector2Int direction) {
            List<Vector2Int> result = new() {
                startPos
            };

            int positionModifier = 1;

            int breakout = 0;
            while (result.Count < unitAmount && breakout < 50) {
                Vector2Int calculatedPos = startPos + direction * positionModifier;

                if (GridStaticFunctions.CurrentBattleGrid.ContainsKey(calculatedPos)) {
                    if (GridStaticFunctions.Grid[calculatedPos].Type != TileType.Normal)
                        continue;

                    result.Add(calculatedPos);
                    direction = -direction;

                    if (direction.x + direction.y > 0)
                        positionModifier++;
                }
                else {
                    bool hasFoundPos = false;
                    List<Vector2Int> tilesToCheck = new() {
                        calculatedPos
                    };
                    List<Vector2Int> checkedTiles = new();

                    int breakout2 = 0;
                    while (!hasFoundPos && breakout2 < 50) {
                        GridStaticFunctions.RippleThroughFullGridPositions(tilesToCheck[0], 2, (tile, i) => {
                            if (hasFoundPos)
                                return;

                            if (!tilesToCheck.Contains(tile) && !checkedTiles.Contains(tile))
                                tilesToCheck.Add(tile);

                            if (!GridStaticFunctions.CurrentBattleGrid.ContainsKey(tile))
                                return;

                            if (result.Contains(tile))
                                return;

                            if (GridStaticFunctions.Grid[tile].Type == TileType.Normal) {
                                hasFoundPos = true;

                                result.Add(tile);
                                direction = -direction;
                            }
                        }, false);

                        checkedTiles.Add(tilesToCheck[0]);
                        tilesToCheck.RemoveAt(0);
                        breakout2++;
                    }

                    if (breakout2 > 49)
                        Debug.Log("Broke out at loop 2");
                }

                breakout++;
            }

            if (breakout > 49)
                Debug.Log("Broke out at Loop 1");

            return result;
        }

        Vector2Int difference = data.PlayerPos - data.EnemyPos;
        Vector2Int middlePoint = data.PlayerPos + -difference / 2;
        Vector2Int direction = Mathf.Abs(difference.x) > Mathf.Abs(difference.y) ? new(0, 1) : new(1, 0);

        List<Vector2Int> points = new();
        for (int x = -((BattleMapSize - 1) / 2); x <= ((BattleMapSize - 1) / 2); x++) {
            for (int y = -((BattleMapSize - 1) / 2); y <= ((BattleMapSize - 1) / 2); y++)
                points.Add(middlePoint + new Vector2Int(x, y));
        }

        GridStaticFunctions.SetBattleGrid(points);

        GridStaticFunctions.PlayerSpawnPositions.AddRange(GetSpawnPositions(data.PlayerUnits.Count, data.PlayerPos, direction));
        GridStaticFunctions.EnemySpawnPositions.AddRange(GetSpawnPositions(data.EnemyUnits.Count, data.EnemyPos, direction));
    }

    private void SpawnUnits(List<UnitData> playerUnitsToSpawn, List<UnitData> enemyUnitsToSpawn) {
        unitHolder = new GameObject("UnitHolder").transform;

        PlayerTurnController playerTurnController = new();
        for (int i = 0; i < GridStaticFunctions.PlayerSpawnPositions.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.PlayerSpawnPositions[i];

            UnitController unit = unitFactory.CreateUnit(playerUnitsToSpawn[i], spawnPos, PlayerUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(new(1, 0));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.PlayerUnitsInPlay.Add(unit);
        }
        playerTurnController.SetUp(new(UnitStaticManager.PlayerUnitsInPlay));
        players.Add(playerTurnController);

        EnemyTurnController enemyTurnController = new();
        for (int i = 0; i < GridStaticFunctions.EnemySpawnPositions.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPositions[i];

            UnitController unit = unitFactory.CreateUnit(enemyUnitsToSpawn[i], spawnPos, EnemyUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(new(-1, 0));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.EnemyUnitsInPlay.Add(unit);
        }
        enemyTurnController.SetUp(new(UnitStaticManager.EnemyUnitsInPlay));
        players.Add(enemyTurnController);
    }

    private void NextPlayer() {
        if (currentPlayerIndex > players.Count - 1) {
            EventManager<BattleEvents>.Invoke(BattleEvents.NewTurn);

            currentPlayerIndex = 0;
        }

        currentPlayer?.OnExit();
        currentPlayer = players[currentPlayerIndex];

        //bool hasUnitsLeft = false;
        //foreach (var unit in CurrentPlayer.Units) {
        //    if (!UnitStaticManager.DeadUnitsInPlay.Contains(unit))
        //        hasUnitsLeft = true;
        //}

        //if (!hasUnitsLeft) {
        //    EventManager<BattleEvents>.Invoke(BattleEvents.BattleEnd);
        //    return;
        //}

        currentPlayer.OnEnter();
        currentPlayerIndex++;
    }

    private void CleanupAfterBattle() {
        currentPlayer = null;
        players.Clear();
        currentPlayerIndex = 0;

        Destroy(unitHolder.gameObject);
        GridStaticFunctions.ResetAllTileColors();
        GridStaticFunctions.PlayerSpawnPositions.Clear();
        GridStaticFunctions.EnemySpawnPositions.Clear();

        UnitStaticManager.Reset();
    }

    private int BattleMapSize => GridStaticFunctions.BattleMapSize;
}

public enum BattleEvents {
    EnemyViewedPlayer,
    StartBattle,
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
