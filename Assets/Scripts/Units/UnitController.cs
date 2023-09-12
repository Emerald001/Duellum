using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour {
    public UnitData UnitBaseData { get; private set; }

    public bool IsDone { get; private set; }
    public UnitValues Values => values;

    private UnitValues values;
    private UnitMovementComponent unitMovementComponent;
    private Vector2Int gridPosition;

    private ActionQueue queue = new();

    [HideInInspector] public List<UnitAttack> abilities = new();
    [HideInInspector] public UnitAttack pickedAttack;

    private bool didAttack;

    public void SetUp(UnitData data, Vector2Int pos) {
        UnitBaseData = Instantiate(data);

        values = new(UnitBaseData);
        unitMovementComponent = new();

        gridPosition = pos;

        queue = new(() => IsDone = ShouldEndTurn());
    }

    public virtual void OnUpdate() {
        queue.OnUpdate();
    }

    public void OnEnter() {
        unitMovementComponent.FindAccessibleTiles(gridPosition, values.currentStats.Speed);
    }

    public virtual void OnExit() {
        if (pickedAttack != null)
            pickedAttack.OnExit();

        ResetTiles();

        pickedAttack = null;
        IsDone = false;
    }

    public virtual void PickedTile(Vector2Int pickedTile, Vector2Int standingPos_optional) {
        //if (AttackableTiles.Contains(pickedTile)) {
        //    if (gridPos == standingPos_optional) {
        //        ActionQueue.Enqueue(new UnitAttack(this, Unit, EnemyPositions[pickedTile], values.damageValue));
        //        ResetTiles();
        //    }
        //    else {
        //        ActionQueue.Enqueue(new UnitMoveToTile(this, pathfinding.FindPathToTile(gridPos, standingPos_optional, TileParents)));
        //        ActionQueue.Enqueue(new UnitAttack(this, Unit, EnemyPositions[pickedTile], values.damageValue));
        //        ResetTiles();
        //    }
        //}
        //else if (AccessableTiles.Contains(pickedTile)) {
        //    ActionQueue.Enqueue(new UnitMoveToTile(this, pathfinding.FindPathToTile(gridPos, pickedTile, TileParents)));
        //    ResetTiles();
        //}
    }

    public virtual void FindTiles() {
        //GridStaticFunctions.UnitPositions.First(x => x.Item2 == this) = gridPos;

        unitMovementComponent.FindAccessibleTiles(gridPosition, values.currentStats.Speed);
    }

    public virtual void ResetTiles() {
        pickedAttack = null;
    }

    public void AddEffect(Effect effect) {
        values.AddEffect(effect);
    }

    private void LowerAbilityCooldowns() {
        foreach (var ability in abilities) {
            if (ability.CurrentCooldown > 0)
                ability.CurrentCooldown--;
        }
    }

    public virtual void SelectAttack(int index) {
        if (pickedAttack == abilities[index]) {
            pickedAttack.OnExit();
            pickedAttack = null;
        }
        else {
            if (pickedAttack != null) {
                pickedAttack.OnExit();
            }
            pickedAttack = abilities[index];

            //if (pickedAttack.targetAnything)
            //    pickedAttack.OnEnter(this, turnManager.LivingUnitsInPlay);
            //else if (pickedAttack.targetEnemy)
            //    pickedAttack.OnEnter(this, EnemyList);
            //else
            //    pickedAttack.OnEnter(this, OwnList);
        }
    }

    private bool ShouldEndTurn() {
        bool speedDown = values.currentStats.Speed < 1;

        return speedDown || didAttack;
    }
}

public class UnitValues {
    public UnitValues(UnitData data) {
        currentStats = new(data.BaseStatBlock);
        baseData = data;

        EventManager<BattleEvents>.Subscribe(BattleEvents.NewTurn, ApplyEffects);
    }

    [HideInInspector] public int Morale;

    public StatBlock currentStats;
    public List<Effect> CurrentEffects;
    
    private UnitData baseData;

    public void AddEffect(Effect effect) {
        if (effect.canBeStacked)
            CurrentEffects.Add(effect);
        else if (!CurrentEffects.Select(x => x.type).Contains(effect.type))
            CurrentEffects.Add(effect);
        else
            CurrentEffects.First(x => x.type == effect.type).duration = Mathf.Max(CurrentEffects.First(x => x.type == effect.type).duration, effect.duration);
    }

    private void ApplyEffects() {
        currentStats = new(baseData.BaseStatBlock);
        EffectsManager.ApplyEffects(this, CurrentEffects);
    }
}

public class UnitMovementComponent {
    public bool IsDone { get; private set; }

    private readonly ActionQueue movementQueue;
    private readonly Dictionary<Vector2Int, Vector2Int> parentDictionary = new();
    private readonly List<Vector2Int> currentAccessableTiles = new();

    public UnitMovementComponent() {
        movementQueue = new(() => IsDone = true);
    }

    public void FindAccessibleTiles(Vector2Int gridPos, int speedValue) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new(); // Use HashSet for faster membership checks

        openList.Add(gridPos);

        for (int i = 0; i < speedValue; i++) {
            foreach (var currentPos in openList.ToList()) { // Use ToList() to avoid modification during iteration
                GridStaticFunctions.RippleThroughSquareGridPositions(currentPos, 2, (neighbour, count) => {
                    if (neighbour == currentPos)
                        return;

                    if (GridStaticFunctions.TryGetUnitFromGridPos(neighbour, out var tmp))
                        return;

                    if (!GridStaticFunctions.Grid[neighbour].CompareTag("WalkableTile") ||
                        openList.Contains(neighbour) ||
                        closedList.Contains(neighbour) ||
                        layerList.Contains(neighbour))
                        return;

                    layerList.Add(neighbour);
                    currentAccessableTiles.Add(neighbour);
                    parentDictionary[neighbour] = currentPos;
                });

                closedList.Add(currentPos);
            }

            openList.Clear();
            openList.AddRange(layerList); // Use ForEach for adding elements from one list to another
            layerList.Clear();
        }

        GridStaticFunctions.HighlightTiles(currentAccessableTiles);
    }

    private void Move() {
        IsDone = false;
    }
}