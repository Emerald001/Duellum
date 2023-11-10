using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : UnitController {
    public LineRenderer Line { get; set; }

    private List<Vector2Int> CurrentPath = new();
    private bool isPerformingAction = false;

    public override void SetUp(UnitData data, Vector2Int pos) {
        base.SetUp(data, pos);

        Line = GetComponent<LineRenderer>();
    }

    public override void OnEnter() {
        base.OnEnter();
        EventManager<BattleEvents>.Subscribe(BattleEvents.ReleasedAbilityCard, FindTiles);

        HighlightTiles();
    }

    public override void OnUpdate() {
        if (Line != null && !isPerformingAction)
            CreatePathForLine();

        base.OnUpdate();

        RunAttack();
    }

    private void RunAttack() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            PickedTile(MouseToWorldView.HoverTileGridPos, attackModule.GetClosestTile(MouseToWorldView.HoverTileGridPos, gridPosition, MouseToWorldView.HoverPointPos));

            GridStaticFunctions.ResetTileColors();
            Line.enabled = false;
            isPerformingAction = true;
        }
    }

    public override void OnExit() {
        base.OnExit();

        isPerformingAction = false;

        Line.enabled = false;
        Tooltip.instance.HideTooltip();

        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.ReleasedAbilityCard, FindTiles);
    }

    public void HighlightTiles() {
        GridStaticFunctions.HighlightTiles(unitMovement.AccessableTiles, HighlightType.MovementHighlight);
        GridStaticFunctions.HighlightTiles(attackModule.AttackableTiles, HighlightType.AttackHighlight);
        GridStaticFunctions.Grid[gridPosition].SetHighlight(HighlightType.OwnPositionHighlight);
    }

    public override void FindTiles() {
        base.FindTiles();

        HighlightTiles();
    }

    private void CreatePathForLine() {
        Vector2Int endPos = MouseToWorldView.HoverTileGridPos;
        // Should Not be here!
        EventManager<UIEvents, string>.Invoke(UIEvents.InfoTextUpdate, "Right mouse button to cancel action");

        if (unitMovement.AccessableTiles.Contains(endPos)) {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Move);
            CurrentPath = unitMovement.GetPath(endPos);
        }
        else if (attackModule.AttackableTiles.Contains(endPos)) {
            CurrentPath = unitMovement.GetPath(attackModule.GetClosestTile(endPos, gridPosition, MouseToWorldView.HoverPointPos));
            CurrentPath.Add(MouseToWorldView.HoverTileGridPos);

            Vector2Int calculatedLookDir = endPos - attackModule.GetClosestTile(endPos, gridPosition, MouseToWorldView.HoverPointPos);
            calculatedLookDir.Clamp(new(-1, -1), new(1, 1));

            // Should not be here!
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Attack);
            if (UnitStaticManager.TryGetUnitFromGridPos(endPos, out var unit)) {
                int bonus = DamageManager.CaluculateDamage(this, unit, calculatedLookDir);
                int damage = DamageManager.CalculateRegularDamage(this, unit);

                if (damage + bonus > unit.UnitBaseData.BaseStatBlock.Defence)
                    Tooltip.instance.ShowTooltip($"<color=green> WIN <br> base damage: {damage}  bonus: + {bonus}</color>");
                else
                    Tooltip.instance.ShowTooltip($"<color=red> FAIL <br> base damage: {damage}  bonus: + {bonus}</color>");
            }
        }
        else {
            Tooltip.instance.HideTooltip();

            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
            Line.enabled = false;
            return;
        }

        DrawPathWithLine();
    }

    private void DrawPathWithLine() {
        Line.enabled = true;

        if (CurrentPath != null && CurrentPath.Count > 0) {
            Line.positionCount = CurrentPath.Count + 1;
            for (int i = 0; i < CurrentPath.Count + 1; i++) {
                if (i == 0) {
                    Line.SetPosition(0, GridStaticFunctions.CalcSquareWorldPos(gridPosition));
                    continue;
                }
                Line.SetPosition(i, GridStaticFunctions.CalcSquareWorldPos(CurrentPath[i - 1]));
            }
        }
    }
}
