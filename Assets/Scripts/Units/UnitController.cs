using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UIElements;

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
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.UnitHit, UnitHit);
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.UnitRevive, UnitRevive);
    }

    private void OnDisable() {
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitDeath, UnitDeath);
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitDeath, UnitHit);
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitDeath, UnitRevive);
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
        GameObject card = Instantiate(data.UnitCard);
        card.transform.localPosition = new Vector3(pos.x + pawn.transform.position.x, 0, pos.y + pawn.transform.position.z);
        
        CharacterCard cardScript = card.GetComponent<CharacterCard>();
        cardScript.name = data.name;
        cardScript.descriptionText.SetText(data.Description.ToString());
        cardScript.defenseText.SetText(data.BaseStatBlock.Defence.ToString()); 
        cardScript.attackText.SetText(data.BaseStatBlock.Attack.ToString());
        cardScript.visuals.sprite = data.Icon;

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

            FindTiles();
            GridStaticFunctions.ResetTileColors();
        }));

        HasPerformedAction = true;
    }

    private void EnqueueAttack(Vector2Int targetPosition, Vector2Int standingPos) {
        Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcSquareWorldPos(targetPosition) - GridStaticFunctions.CalcSquareWorldPos(standingPos));

        queue.Enqueue(new RotateAction(gameObject, new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection), 0), 360f, .01f));
        queue.Enqueue(new DoMethodAction(() => unitAnimator.SetTrigger("Attacking")));
        queue.Enqueue(new WaitAction(.2f));
        queue.Enqueue(new DoMethodAction(() => {
            bool hit = UnitStaticManager.TryGetUnitFromGridPos(targetPosition, out var unit);
            if (!hit)
                throw new System.Exception("Something went Very wrong with getting the units attackable tiles");

            DamageManager.DealDamage(values, unit);
        }));
        HasPerformedAction = true;
    }

    private void UnitHit(UnitController unit) {
        unit.unitAnimator.SetTrigger("GettingHit");
        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, 0.1f);
    }

    private void UnitDeath(UnitController unit) {
        unit.unitAnimator.SetTrigger("Dying");
    }

    private void UnitRevive(UnitController unit) {
        unit.unitAnimator.SetTrigger("Reviving");
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
