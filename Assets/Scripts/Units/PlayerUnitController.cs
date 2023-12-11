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
            if (!attackModule.AttackableTiles.Contains(MouseToWorldView.HoverTileGridPos) &&
                !movementModule.AccessableTiles.Contains(MouseToWorldView.HoverTileGridPos))
                return;

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
        Tooltip.HideTooltip_Static();

        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.ReleasedAbilityCard, FindTiles);
    }

    public void HighlightTiles() {
        GridStaticFunctions.HighlightTiles(movementModule.AccessableTiles, HighlightType.MovementHighlight);
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

        if (movementModule.AccessableTiles.Contains(endPos)) {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Move);
            CurrentPath = movementModule.GetPath(endPos);
        }
        else if (attackModule.AttackableTiles.Contains(endPos)) {
            CurrentPath = movementModule.GetPath(attackModule.GetClosestTile(endPos, gridPosition, MouseToWorldView.HoverPointPos));
            CurrentPath.Add(MouseToWorldView.HoverTileGridPos);

            Vector2Int calculatedLookDir = endPos - attackModule.GetClosestTile(endPos, gridPosition, MouseToWorldView.HoverPointPos);
            calculatedLookDir.Clamp(new(-1, -1), new(1, 1));

            // Should not be here!
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Attack);
            if (UnitStaticManager.TryGetUnitFromGridPos(endPos, out var unit)) {
                int bonus = DamageManager.CaluculateDamage(this, unit, calculatedLookDir);
                int damage = Values.currentStats.Attack;

                if (damage + bonus > unit.UnitBaseData.BaseStatBlock.Defence)
                    Tooltip.ShowTooltip_Static($"<color=green> WIN <br> base damage: {damage}  bonus: + {bonus}</color>");
                else
                    Tooltip.ShowTooltip_Static($"<color=red> FAIL <br> base damage: {damage}  bonus: + {bonus}</color>");
            }
        }
        else {
            Tooltip.HideTooltip_Static();

            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
            Line.enabled = false;
            return;
        }

        DrawPathWithLine();
    }

    private void DrawPathWithLine() {
        if (CurrentPath == null || CurrentPath.Count <= 0)
            return;

        Line.enabled = true;
        Line.positionCount = CurrentPath.Count + 1;

        Line.SetPosition(0, GridStaticFunctions.CalcSquareWorldPos(gridPosition));
        for (int i = 1; i < CurrentPath.Count + 1; i++)
            Line.SetPosition(i, GridStaticFunctions.CalcSquareWorldPos(CurrentPath[i - 1]));
    }
}
