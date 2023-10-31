using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI infoText;

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

    private void OnEnable() {
        EventManager<BattleEvents, string>.Subscribe(BattleEvents.InfoTextUpdate, UpdateInfoUI);    
    }

    private void OnDisable() {
        EventManager<BattleEvents, string>.Unsubscribe(BattleEvents.InfoTextUpdate, UpdateInfoUI);    
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
            Canvas canvas = card.GetComponentInChildren<Canvas>();
            Vector3 originalSpawnPos = GridStaticFunctions.CalcSquareWorldPos(spawnPos);
            Vector3 cardPos = new Vector3(originalSpawnPos.x, originalSpawnPos.y, originalSpawnPos.z);
            canvas.transform.position = cardPos;

            float spaceToGrid = 12f;
            Vector3 localOffset = new Vector3(0, spaceToGrid, 0);
            canvas.transform.localPosition -= localOffset;

            CharacterCard cardScript = card.GetComponent<CharacterCard>();
            cardScript.name = unit.UnitBaseData.name;
            cardScript.descriptionText.SetText(unit.UnitBaseData.Description.ToString());
            cardScript.defenseText.SetText(unit.UnitBaseData.BaseStatBlock.Defence.ToString());
            cardScript.attackText.SetText(unit.UnitBaseData.BaseStatBlock.Attack.ToString());
            cardScript.visuals.sprite = unit.UnitBaseData.Icon;
        }
        players.Add(new PlayerTurnController());

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPos[i];

            var unit = unitFactory.CreateUnit(EnemyUnitPrefab, EnemyUnitsToSpawn[i], spawnPos);
            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.EnemyUnitsInPlay.Add(unit);

            GameObject card = Instantiate(unit.UnitBaseData.UnitCard, GameObject.Find("UnitCards").transform);
            Canvas canvas = card.GetComponentInChildren<Canvas>();
            Vector3 originalSpawnPos = GridStaticFunctions.CalcSquareWorldPos(spawnPos);
            Vector3 cardPos = new Vector3(originalSpawnPos.x, originalSpawnPos.y, originalSpawnPos.z);
            canvas.transform.position = cardPos;

            float spaceToGrid = 12f;
            Vector3 localOffset = new Vector3(0, spaceToGrid, 0);
            canvas.transform.localPosition += localOffset;
            Quaternion oppositeRotation = Quaternion.Euler(0, 0, 180);
            canvas.transform.localRotation = transform.localRotation * oppositeRotation;

            CharacterCard cardScript = card.GetComponent<CharacterCard>();
            cardScript.name = unit.UnitBaseData.name;
            cardScript.descriptionText.SetText(unit.UnitBaseData.Description.ToString());
            cardScript.defenseText.SetText(unit.UnitBaseData.BaseStatBlock.Defence.ToString());
            cardScript.attackText.SetText(unit.UnitBaseData.BaseStatBlock.Attack.ToString());
            cardScript.visuals.sprite = unit.UnitBaseData.Icon;
        }
        players.Add(new EnemyTurnController());

        //GameObject unitGridParent = GridStaticFunctions.GetGameObjectAtPosition(pos);
        //Canvas canvas = card.GetComponentInChildren<Canvas>();

        //if (!GridStaticFunctions.EnemySpawnPos.Contains(gridPosition)) {
        //    float padding = 12f;
        //    Vector3 localOffset = new Vector3(0, padding, 0);
        //    canvas.transform.localPosition -= localOffset;
        //    return;
        //}
        //else {

        //}


    }

    private void UpdateInfoUI(string name) {
        infoText.text = name;
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
