using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {
    [SerializeField] private GridCardManager cardManager;

    [SerializeField] private UnitController PlayerUnitPrefab;
    [SerializeField] private UnitController EnemyUnitPrefab;

    // Wish we had a better way of doing this
    [SerializeField] private CardHand enemyCardHand;

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
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, CleanupAfterBattle);
    }
    private void OnDisable() {
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, CleanupAfterBattle);
    }

    public void StartBattle(BattleData data) {
        SpawnUnits(data.PlayerUnits, data.EnemyUnits);
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
        enemyTurnController.CardHand = enemyCardHand;
        for (int i = 0; i < GridStaticFunctions.EnemySpawnPositions.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPositions[i];

            UnitController unit = unitFactory.CreateUnit(enemyUnitsToSpawn[i], spawnPos, EnemyUnitPrefab, unitHolder);
            unit.ChangeUnitRotation(new(-1, 0));

            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.EnemyUnitsInPlay.Add(unit);
        }
        enemyTurnController.SetUp(UnitStaticManager.EnemyUnitsInPlay);
        players.Add(enemyTurnController);
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
    PickUpAbilityCard,
    SpawnAbilityCard,
    BattleEnd,
}
