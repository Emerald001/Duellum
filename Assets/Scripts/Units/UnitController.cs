﻿using System.Collections.Generic;
using UnityEngine;

public abstract class UnitController : MonoBehaviour {
    [SerializeField] private GameObject pawnParent;

    public UnitData UnitBaseData { get; private set; }
    public bool HasPerformedAction { get; private set; }
    public bool IsDone { get; private set; }
    public int OwnerID { get; private set; }

    public UnitValues Values => values;
    protected UnitValues values;

    public Vector2Int LookDirection => lookDirection;
    protected Vector2Int lookDirection;

    protected UnitMovementModule movementModule;
    protected UnitAttackModule attackModule;
    protected Vector2Int gridPosition;

    private ActionQueue queue;
    private Animator unitAnimator;

    private void OnEnable() {
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.UnitDeath, UnitDeath);
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.UnitHit, UnitHit);
        EventManager<BattleEvents, UnitController>.Subscribe(BattleEvents.UnitRevive, UnitRevive);
    }
    private void OnDisable() {
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitDeath, UnitDeath);
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitHit, UnitHit);
        EventManager<BattleEvents, UnitController>.Unsubscribe(BattleEvents.UnitRevive, UnitRevive);
    }

    public virtual void SetUp(int id, UnitData data, Vector2Int pos) {
        UnitBaseData = Instantiate(data);
        GameObject pawn = Instantiate(data.PawnPrefab, transform);

        OwnerID = id;

        values = new(UnitBaseData);
        movementModule = new();
        attackModule = new(UnitBaseData.Attack, id);
        gridPosition = pos;
        queue = new(() => IsDone = HasPerformedAction);

        unitAnimator = GetComponentInChildren<Animator>();
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
        IsDone = false;

        Debug.Log(3);
    }

    protected virtual void PickedTile(Vector2Int pickedTile, Vector2Int standingPos_optional) {
        if (attackModule.AttackableTiles.Contains(pickedTile)) {
            if (gridPosition == standingPos_optional)
                EnqueueAttack(pickedTile, standingPos_optional);
            else {
                EnqueueMovement(standingPos_optional);
                EnqueueAttack(pickedTile, standingPos_optional);
            }
        }
        else if (movementModule.AccessableTiles.Contains(pickedTile))
            EnqueueMovement(pickedTile);
    }

    private void EnqueueMovement(Vector2Int targetPosition) {
        queue.Enqueue(new DoMethodAction(() => {
            unitAnimator.SetBool("Walking", true);
            //UnitAudio.PlayLoopedAudio("Walking", true);
        }));

        Vector2Int lastPos = gridPosition;
        foreach (var newPos in movementModule.GetPath(targetPosition)) {
            Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcWorldPos(newPos) - GridStaticFunctions.CalcWorldPos(lastPos));

            queue.Enqueue(new ActionStack(
                new MoveObjectAction(gameObject, UnitBaseData.movementSpeed, GridStaticFunctions.CalcWorldPos(newPos)),
                new RotateAction(gameObject, new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection), 0), 360f, .01f)
                ));

            queue.Enqueue(new DoMethodAction(() => {
                this.lookDirection = lookDirection;
                UnitStaticManager.SetUnitPosition(this, newPos);
                gridPosition = newPos;
                values.currentStats.Speed--;

                if (GridStaticFunctions.TileEffectPositions.ContainsKey(newPos))
                    EventManager<BattleEvents, EventMessage<UnitController, Vector2Int>>.Invoke(BattleEvents.UnitTouchedTileEffect, new(this, newPos));
            }));

            lastPos = newPos;
        }

        queue.Enqueue(new DoMethodAction(() => {
            unitAnimator.SetBool("Walking", false);
        }));

        HasPerformedAction = true;
    }

    private void EnqueueAttack(Vector2Int targetPosition, Vector2Int standingPos) {
        Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcWorldPos(targetPosition) - GridStaticFunctions.CalcWorldPos(standingPos));

        queue.Enqueue(new RotateAction(gameObject, new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection), 0), 360f, .01f));
        queue.Enqueue(new DoMethodAction(() => unitAnimator.SetTrigger("Attacking")));
        queue.Enqueue(new WaitAction(.2f));
        queue.Enqueue(new DoMethodAction(() => {
            bool hit = UnitStaticManager.TryGetUnitFromGridPos(targetPosition, out var unit);
            if (!hit)
                throw new System.Exception("Something went Very wrong with getting the units attackable tiles");

            this.lookDirection = lookDirection;
            DamageManager.DealDamage(this, unit);
        }));
        HasPerformedAction = true;
    }

    public virtual void FindTiles() {
        movementModule.FindAccessibleTiles(gridPosition, values.currentStats.Speed);

        List<Vector2Int> tiles = new(movementModule.AccessableTiles) {
            gridPosition
        };
        attackModule.FindAttackableTiles(tiles, UnitStaticManager.GetEnemies(OwnerID));
    }

    public void AddEffect(Effect effect) {
        values.AddEffect(effect);
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

    public void ChangeUnitPosition(Vector2Int newPosition) {
        UnitStaticManager.SetUnitPosition(this, newPosition);

        transform.position = GridStaticFunctions.CalcWorldPos(newPosition);

        gridPosition = newPosition;
    }

    public void ChangeUnitRotation(Vector2Int newRotation) {
        lookDirection = newRotation;

        transform.rotation = Quaternion.Euler(new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection), 0));
    }
}
