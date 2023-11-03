using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {
    [SerializeField] private UnitController PlayerUnitPrefab;
    [SerializeField] private UnitController EnemyUnitPrefab;

    [SerializeField] private List<UnitData> PlayerUnitsToSpawn;
    [SerializeField] private List<UnitData> EnemyUnitsToSpawn;

    [Header("GridSettings")]
    [SerializeField] private OriginalMapGenerator GridGenerator;
    [SerializeField] private GridCardManager GridCardManager;

    public TurnController CurrentPlayer => currentPlayer;
    public bool IsDone { get; private set; }

    private UnitFactory unitFactory;

    private List<TurnController> players = new();
    private TurnController currentPlayer;
    private int currentPlayerIndex;

    private void Awake() {
        unitFactory = new();

        GridGenerator.SetUp();
        GridCardManager.SetUp();

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

            GameObject card = Instantiate(unit.UnitBaseData.UnitCard, GameObject.Find("UnitCards").transform);
            card.transform.position = GridStaticFunctions.CalcSquareWorldPos(spawnPos) + new Vector3(-3f, 0, (i - 1) * 2);

            CharacterCard cardScript = card.GetComponent<CharacterCard>();
            cardScript.SetUp(PlayerUnitsToSpawn[i]);
        }
        players.Add(new PlayerTurnController());

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPos[i];

            UnitController unit = unitFactory.CreateUnit(EnemyUnitPrefab, EnemyUnitsToSpawn[i], spawnPos);
            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.EnemyUnitsInPlay.Add(unit);

            GameObject card = Instantiate(unit.UnitBaseData.UnitCard, GameObject.Find("UnitCards").transform);
            card.transform.position = GridStaticFunctions.CalcSquareWorldPos(spawnPos) + new Vector3(3f, 0, -((i - 1) * 4f));
            card.transform.rotation = Quaternion.Euler(90, 0, 90);

            CharacterCard cardScript = card.GetComponent<CharacterCard>();
            cardScript.SetUp(EnemyUnitsToSpawn[i]);
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

//I don't know how to add new enum for events and link them to the eventmanager, so for now I'm gonna put them in battle events.
public enum BattleEvents {
    SetupBattle,
    StartBattle,
    EndUnitTurn,
    NewTurn,
    UnitHit,
    UnitDeath,
    InfoTextUpdate,
    UnitRevive,
    GiveAbilityCard,
    SpawnAbilityCard,
    GrabbedAbilityCard,
    ReleasedAbilityCard,
    BattleEnd
}
