using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private UnitController UnitPrefab;

    [SerializeField] private List<UnitData> PlayerUnitsToSpawn;
    [SerializeField] private List<UnitData> EnemyUnitsToSpawn;

    [Header("GridSettings")]
    [SerializeField] private OriginalMapGenerator GridGenerator;

    private List<UnitController> unitAttackOrder = new();
    private UnitController currentUnit;

    private int currentTurn = 0;

    private UnitFactory unitFactory;

    public bool IsDone { get; private set; }

    private void Awake() {
        unitFactory = new(UnitPrefab);

        GridGenerator.SetUp();

        SpawnUnits();
        NextTurn();
    }

    private void Update() {
        if (currentUnit == null)
            return;

        if (currentUnit.IsDone) {
            NextUnit();
            return;
        }

        currentUnit.OnUpdate();
    }

    private void SpawnUnits() {
        for (int i = 0; i < GridStaticFunctions.PlayerSpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.PlayerSpawnPos[i];

            var unit = unitFactory.CreateUnit(PlayerUnitsToSpawn[i], spawnPos);
            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.PlayerUnitsInPlay.Add(unit);
        }

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int spawnPos = GridStaticFunctions.EnemySpawnPos[i];

            var unit = unitFactory.CreateUnit(EnemyUnitsToSpawn[i], spawnPos);
            UnitStaticManager.SetUnitPosition(unit, spawnPos);
            UnitStaticManager.LivingUnitsInPlay.Add(unit);
            UnitStaticManager.EnemyUnitsInPlay.Add(unit);
        }
    }

    private void OrderUnits() {
        unitAttackOrder = new List<UnitController>(UnitStaticManager.UnitsWithTurnLeft.OrderBy(x => x.Values.currentStats.Initiative).Reverse());
    }

    private void NextUnit() {
        if (unitAttackOrder.Count != 0) {
            //for (int i = 0; i < unitAttackOrder.Count; i++)
            //    unitAttackOrder[i].gameObject.GetComponentInChildren<Text>().text = (i + 1).ToString();

            if (currentUnit != null) {
                currentUnit.OnExit();
                //currentUnit.gameObject.GetComponentInChildren<Text>().text = "-";
            }

            currentUnit = unitAttackOrder[0];
            unitAttackOrder.RemoveAt(0);
            UnitStaticManager.UnitsWithTurnLeft.Remove(currentUnit);
            currentUnit.OnEnter();
        }
        else {
            if (currentUnit != null)
                currentUnit.OnExit();

            currentUnit = null;
            NextTurn();
        }
    }

    private void NextTurn() {
        foreach (var unit in UnitStaticManager.LivingUnitsInPlay)
            UnitStaticManager.UnitsWithTurnLeft.Add(unit);

        currentTurn++;

        //UIManager.SetInfoText("Turn " + currentTurn.ToString());

        EventManager<BattleEvents>.Invoke(BattleEvents.NewTurn);

        OrderUnits();
        NextUnit();
    }

    private void OnExit() {
        if (UnitStaticManager.EnemyUnitsInPlay.Count < 1) {
            //var header = "Win";
            //var body = "You won in " + currentTurn + " turns.";

            //UIManager.ShowEndScreen(header, body);
        }
        else if (UnitStaticManager.PlayerUnitsInPlay.Count < 1) {
            //var header = "Lose";
            //var body = "You lost in " + currentTurn + " turns.";

            //UIManager.ShowEndScreen(header, body);
        }
    }

    public void UnitDeath(UnitController unit) {
        UnitStaticManager.LivingUnitsInPlay.Remove(unit);
        UnitStaticManager.DeadUnitsInPlay.Add(unit);

        if (unitAttackOrder.Contains(unit)) {
            UnitStaticManager.UnitsWithTurnLeft.Remove(unit);
            unitAttackOrder.Remove(unit);
        }

        OrderUnits();

        if (UnitStaticManager.PlayerUnitsInPlay.Contains(unit)) {
            UnitStaticManager.PlayerUnitsInPlay.Remove(unit);

            if (UnitStaticManager.PlayerUnitsInPlay.Count < 1)
                OnExit();
                //StartCoroutine(ExitDelay(2));
        }
        else if (UnitStaticManager.EnemyUnitsInPlay.Contains(unit)) {
            UnitStaticManager.EnemyUnitsInPlay.Remove(unit);

            if (UnitStaticManager.EnemyUnitsInPlay.Count < 1)
                OnExit();
                //StartCoroutine(ExitDelay(2));
        }
    }

    public void UnitWait() {
        if (currentUnit.Values.currentStats.Initiative < 1)
            return;

        var curUnit = currentUnit;
        NextUnit();
        UnitStaticManager.UnitsWithTurnLeft.Add(curUnit);
        curUnit.Values.currentStats.Initiative *= -1;
        OrderUnits();

        for (int i = 0; i < unitAttackOrder.Count; i++)
            unitAttackOrder[i].gameObject.GetComponentInChildren<Text>().text = (i + 2).ToString();
    }

    public void UnitEndTurn() {
        currentUnit.Values.currentStats.Defence *= 2;
        NextUnit();
    }
}

public class UnitFactory {
    public UnitFactory(UnitController prefab) {
        this.prefab = prefab;
    }

    private readonly UnitController prefab;
    
    public UnitController CreateUnit(UnitData data, Vector2Int spawnPos) {
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
    GrabbedAbilityCard,
    ReleasedAbilityCard,
    BattleEnd,
}