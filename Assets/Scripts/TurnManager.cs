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

    private readonly List<UnitController> livingUnitsInPlay = new();

    private readonly List<UnitController> deadUnitsInPlay = new();
    private readonly List<UnitController> unitsWithTurnLeft = new();

    private readonly List<UnitController> enemyUnitsInPlay = new();
    private readonly List<UnitController> playerUnitsInPlay = new();

    private List<UnitController> unitAttackOrder = new();
    private UnitController currentUnit;

    private int currentTurn;

    private UnitFactory unitFactory;

    public bool IsDone { get; private set; }

    private void Awake() {
        unitFactory = new(UnitPrefab);

        GridGenerator.SetUp();

        SpawnUnits();
        NextUnit();
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
            Vector2Int item = GridStaticFunctions.PlayerSpawnPos[i];
            livingUnitsInPlay.Add(unitFactory.CreateUnit(PlayerUnitsToSpawn[i], item));
        }

        for (int i = 0; i < GridStaticFunctions.EnemySpawnPos.Count; i++) {
            Vector2Int item = GridStaticFunctions.EnemySpawnPos[i];
            livingUnitsInPlay.Add(unitFactory.CreateUnit(EnemyUnitsToSpawn[i], item));
        }
    }

    private void OrderUnits() {
        unitAttackOrder = new List<UnitController>(unitsWithTurnLeft.OrderBy(x => x.Values.currentStats.Initiative).Reverse());
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
            unitsWithTurnLeft.Remove(currentUnit);
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
        foreach (var unit in livingUnitsInPlay)
            unitsWithTurnLeft.Add(unit);

        currentTurn++;

        //UIManager.SetInfoText("Turn " + currentTurn.ToString());

        OrderUnits();
        NextUnit();
    }

    private void OnExit() {
        if (enemyUnitsInPlay.Count < 1) {
            var header = "Win";
            var body = "You won in " + currentTurn + " turns.";

            //UIManager.ShowEndScreen(header, body);
        }
        else if (playerUnitsInPlay.Count < 1) {
            var header = "Lose";
            var body = "You lost in " + currentTurn + " turns.";

            //UIManager.ShowEndScreen(header, body);
        }
    }

    public void UnitDeath(UnitController unit) {
        livingUnitsInPlay.Remove(unit);
        deadUnitsInPlay.Add(unit);

        if (unitAttackOrder.Contains(unit)) {
            unitsWithTurnLeft.Remove(unit);
            unitAttackOrder.Remove(unit);
        }

        OrderUnits();

        if (playerUnitsInPlay.Contains(unit)) {
            playerUnitsInPlay.Remove(unit);

            if (playerUnitsInPlay.Count < 1)
                OnExit();
                //StartCoroutine(ExitDelay(2));
        }
        else if (enemyUnitsInPlay.Contains(unit)) {
            enemyUnitsInPlay.Remove(unit);

            if (enemyUnitsInPlay.Count < 1)
                OnExit();
                //StartCoroutine(ExitDelay(2));
        }
    }

    public void UnitWait() {
        if (currentUnit.Values.currentStats.Initiative < 1)
            return;

        var curUnit = currentUnit;
        NextUnit();
        unitsWithTurnLeft.Add(curUnit);
        curUnit.Values.currentStats.Initiative *= -1;
        OrderUnits();

        for (int i = 0; i < unitAttackOrder.Count; i++) {
            unitAttackOrder[i].gameObject.GetComponentInChildren<Text>().text = (i + 2).ToString();
        }
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
    BattleEnd,
}