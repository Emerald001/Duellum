using System.Collections.Generic;
using UnityEngine;

public class BattleManager : Singleton<BattleManager> {
    [SerializeField] private TileEffectManager cardManager;

    [SerializeField] private UnitController PlayerUnitPrefab;
    [SerializeField] private UnitController EnemyUnitPrefab;

    // Wish we had a better way of doing this
    [SerializeField] private CardHand enemyCardHand;

    public TurnController CurrentPlayer => currentPlayer;
    public bool IsDone { get; private set; }

    private UnitFactory unitFactory;

    private readonly Dictionary<int, TurnController> players = new();
    private TurnController currentPlayer;
    private int currentPlayerIndex;

    private Transform unitHolder;

    private void Awake() {
        Init(this);
        unitFactory = new();
    }

    private void OnEnable() {
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, CleanupAfterBattle);
    }
    private void OnDisable() {
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, CleanupAfterBattle);
    }

    public void StartBattle(BattleData data) {
        Vector2Int difference = data.PlayerPos - data.EnemyPos;
        Vector2Int direction = Mathf.Abs(difference.x) > Mathf.Abs(difference.y) ? new(0, 1) : new(1, 0);
        SpawnUnits(direction, data.PlayerUnits, data.EnemyUnits);

        cardManager.SetUp();

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

    public void SetBattlefield(BattleData data, Vector2Int direction) {
        List<Vector2Int> GetSpawnPositions(int unitAmount, Vector2Int startPos) {
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

        GridStaticFunctions.PlayerSpawnPositions.AddRange(GetSpawnPositions(data.PlayerUnits.Count, data.PlayerPos));
        GridStaticFunctions.EnemySpawnPositions.AddRange(GetSpawnPositions(data.EnemyUnits.Count, data.EnemyPos));
    }

    private void SpawnUnits(Vector2Int direction, List<UnitData> playerUnitsToSpawn, List<UnitData> enemyUnitsToSpawn) {
        unitHolder = new GameObject("UnitHolder").transform;
        Vector2Int reversedDirection = new(direction.y, direction.x);

        PlayerTurnController playerTurnController = new();
        UnitStaticManager.UnitTeams.Add(0, new());
        for (int i = 0; i < GridStaticFunctions.PlayerSpawnPositions.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.PlayerSpawnPositions[i];

            UnitController unit = unitFactory.CreateUnit(0, playerUnitsToSpawn[i], spawnPos, PlayerUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(reversedDirection);

            for (int j = 0; j < playerUnitsToSpawn[i].CardAmount; j++)
                EventManager<UIEvents, EventMessage<int, AbilityCard>>.Invoke(UIEvents.GiveCard, new(0, playerUnitsToSpawn[i].Card));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.UnitTeams[0].Add(unit);
        }
        playerTurnController.SetUp(0, UnitStaticManager.UnitTeams[0]);
        players.Add(0, playerTurnController);

        EnemyTurnController enemyTurnController = new();
        enemyTurnController.CardHand = enemyCardHand;
        enemyCardHand.OwnerID = 1;
        UnitStaticManager.UnitTeams.Add(1, new());
        for (int i = 0; i < GridStaticFunctions.EnemySpawnPositions.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPositions[i];

            UnitController unit = unitFactory.CreateUnit(1, enemyUnitsToSpawn[i], spawnPos, EnemyUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(-reversedDirection);

            for (int j = 0; j < enemyUnitsToSpawn[i].CardAmount; j++)
                EventManager<UIEvents, EventMessage<int, AbilityCard>>.Invoke(UIEvents.GiveCard, new(1, enemyUnitsToSpawn[i].Card));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.UnitTeams[1].Add(unit);
        }
        enemyTurnController.SetUp(1, UnitStaticManager.UnitTeams[1]);
        players.Add(1, enemyTurnController);
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
        GridStaticFunctions.PlayerSpawnPositions.Clear();
        GridStaticFunctions.EnemySpawnPositions.Clear();

        UnitStaticManager.Reset();
    }

    private int BattleMapSize => GridStaticFunctions.BattleMapSize;
}

public enum BattleEvents {
    EnemyViewedPlayer,
    StartPlayerStartSequence,
    StartBattle,
    NewTurn,
    UnitHit,
    UnitDeath,
    UnitRevive,
    UnitTouchedTileEffect,
    SpawnAbilityCard,
    BattleEnd,
}
