using System.Collections.Generic;
using UnityEngine;

public abstract class UnitController : MonoBehaviour {

    private Animator unitAnimator;
    [SerializeField] private GameObject pawnParent;
    public UnitData UnitBaseData { get; private set; }
    public bool HasPerformedAction { get; private set; }
    public bool IsDone { get; private set; }
    
    public UnitValues Values => values;
    protected UnitValues values;

    protected UnitMovementComponent unitMovement;
    protected UnitAttackModule attackModule;
    protected Vector2Int gridPosition;
    protected Vector2Int lookDirection;

    private ActionQueue queue;

    private bool didAttack;

    private void OnEnable() {
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.UnitDeath, UnitDeath);
    }

    private void OnDisable() {
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitDeath, UnitDeath);
    }
    private void Start() {
        
        unitAnimator = GetComponentInChildren<Animator>();
    }
    public virtual void SetUp(UnitData data, Vector2Int pos) {
        UnitBaseData = Instantiate(data);
        GameObject pawn = Instantiate(data.PawnPrefab, pawnParent.transform);
        values = new(UnitBaseData);
        unitMovement = new();
        attackModule = new(UnitBaseData.Attack);

        gridPosition = pos;

        queue = new(() => IsDone = ShouldEndTurn());
    }

    public virtual void OnEnter() {
        IsDone = false;
        FindTiles();
    }

    public virtual void OnUpdate() {
        queue.OnUpdate();
    }

    public virtual void OnExit() {
        HasPerformedAction = false;
        didAttack = false;
        IsDone = false;
    }

    public virtual void PickedTile(Vector2Int pickedTile, Vector2Int standingPos_optional) {
        if (attackModule.AttackableTiles.Contains(pickedTile)) {
            if (gridPosition == standingPos_optional)
                EnqueueAttack(pickedTile, standingPos_optional);
            else {
                EnqueueMovement(standingPos_optional);
                EnqueueAttack(pickedTile, standingPos_optional);
            }

            didAttack = true;
        }
        else if (unitMovement.AccessableTiles.Contains(pickedTile))
            EnqueueMovement(pickedTile);
    }

    private void EnqueueMovement(Vector2Int targetPosition) {
        queue.Enqueue(new DoMethodAction(() => {
            unitAnimator.SetBool("Walking", true);
            //UnitAudio.PlayLoopedAudio("Walking", true);
        }));

        Vector2Int lastPos = gridPosition;
        foreach (var newPos in unitMovement.GetPath(targetPosition)) {
            Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcSquareWorldPos(newPos) - GridStaticFunctions.CalcSquareWorldPos(lastPos));

            queue.Enqueue(new ActionStack(
                new MoveObjectAction(gameObject, UnitBaseData.movementSpeed, GridStaticFunctions.CalcSquareWorldPos(newPos)),
                new RotateAction(gameObject, new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection), 0), 360f, .01f)
                ));

            queue.Enqueue(new DoMethodAction(() => {
                this.lookDirection = lookDirection;
                UnitStaticManager.SetUnitPosition(this, newPos);
                gridPosition = newPos;
                values.currentStats.Speed--;
            }));

            lastPos = newPos;
        }

        queue.Enqueue(new DoMethodAction(() => {
            unitAnimator.SetBool("Walking", false);
            //UnitAudio.PlayLoopedAudio("Walking", false);

            FindTiles();
            GridStaticFunctions.ResetTileColors();
        }));

        HasPerformedAction = true;
    }

    private void EnqueueAttack(Vector2Int targetPosition, Vector2Int standingPos) {
        Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcSquareWorldPos(targetPosition) - GridStaticFunctions.CalcSquareWorldPos(standingPos));

        queue.Enqueue(new RotateAction(gameObject, new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection), 0), 360f, .01f));
        queue.Enqueue(new DoMethodAction(() => {
            bool hit = UnitStaticManager.TryGetUnitFromGridPos(targetPosition, out var unit);
            if (!hit)
                throw new System.Exception("Something went Very wrong with getting the units attackable tiles");

            DamageManager.DealDamage(values, unit);
            unit.unitAnimator.SetTrigger("GettingHit");
            unitAnimator.SetBool("Attacking", true);
            
        }));

        unitAnimator.SetBool("Attacking", false);
        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, 0.1f);
        HasPerformedAction = true;
    }

    private void UnitDeath(UnitController unit) {
        unit.unitAnimator.SetTrigger("Dying");
    }
    public virtual void FindTiles() {
        unitMovement.FindAccessibleTiles(gridPosition, values.currentStats.Speed);

        List<Vector2Int> tiles = new(unitMovement.AccessableTiles) {
            gridPosition
        };
        attackModule.FindAttackableTiles(tiles, UnitStaticManager.GetEnemies(this));
    }

    public void AddEffect(Effect effect) {
        values.AddEffect(effect);
    }

    private bool ShouldEndTurn() {
        bool speedDown = values.currentStats.Speed < 1;
        bool noAttacks = attackModule.AttackableTiles.Count < 1;

        return (speedDown && noAttacks) || didAttack;
    }
}
