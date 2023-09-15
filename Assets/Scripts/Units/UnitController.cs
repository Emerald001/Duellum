using UnityEngine;

public abstract class UnitController : MonoBehaviour {
    public UnitData UnitBaseData { get; private set; }
    public bool IsDone { get; private set; }

    public UnitValues Values => values;
    protected UnitValues values;

    protected UnitMovementComponent unitMovement;
    protected UnitAttackModule attackModule;
    protected Vector2Int gridPosition;

    private ActionQueue queue;

    private bool didAttack;

    public virtual void SetUp(UnitData data, Vector2Int pos) {
        UnitBaseData = Instantiate(data);

        values = new(UnitBaseData);
        unitMovement = new();
        attackModule = new(UnitBaseData.Attack);

        gridPosition = pos;

        queue = new(() => IsDone = ShouldEndTurn());
    }

    public virtual void OnEnter() {
        FindTiles();
    }

    public virtual void OnUpdate() {
        queue.OnUpdate();
    }

    public virtual void OnExit() {
        IsDone = false;
    }

    public virtual void PickedTile(Vector2Int pickedTile, Vector2Int standingPos_optional) {
        if (false) { //AttackableTiles.Contains(pickedTile)) {
            //if (gridPosition == standingPos_optional) {
            //    //queue.Enqueue(new UnitAttack(this, Unit, EnemyPositions[pickedTile], values.damageValue));
            //}
            //else {
            //    EnqueueMovement(standingPos_optional);

            //    //queue.Enqueue(new UnitAttack(this, Unit, EnemyPositions[pickedTile], values.damageValue));
            //}
        }
        else if (unitMovement.AccessableTiles.Contains(pickedTile))
            EnqueueMovement(pickedTile);
    }

    private void EnqueueMovement(Vector2Int targetPosition) {
        queue.Enqueue(new DoMethodAction(() => {
            //UnitAnimator.WalkAnim(true);
            //UnitAudio.PlayLoopedAudio("Walking", true);
        }));

        foreach (var item in unitMovement.GetPath(targetPosition)) {
            Vector2Int newPos = item;
            queue.Enqueue(new MoveObjectAction(gameObject, UnitBaseData.movementSpeed, GridStaticFunctions.CalcSquareWorldPos(item)));
            queue.Enqueue(new DoMethodAction(() => gridPosition = newPos));
            queue.Enqueue(new DoMethodAction(() => values.currentStats.Speed--));
        }

        queue.Enqueue(new DoMethodAction(() => {
            //UnitAnimator.WalkAnim(false);
            //UnitAudio.PlayLoopedAudio("Walking", false);

            FindTiles();
            GridStaticFunctions.ResetTileColors();
        }));
    }

    public virtual void FindTiles() {
        unitMovement.FindAccessibleTiles(gridPosition, values.currentStats.Speed);
    }

    public void AddEffect(Effect effect) {
        values.AddEffect(effect);
    }

    private bool ShouldEndTurn() {
        bool speedDown = values.currentStats.Speed < 1;

        return speedDown || didAttack;
    }
}
