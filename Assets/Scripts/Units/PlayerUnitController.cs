using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : UnitController {
    public LineRenderer Line { get; set; }

    private List<Vector2Int> CurrentPath = new();
    private bool isPerformingAction = false;

    public override void SetUp(int id, UnitData data, Vector2Int pos) {
        base.SetUp(id, data, pos);

        Line = GetComponent<LineRenderer>();
    }

    public override void OnEnter() {
        base.OnEnter();
        EventManager<UIEvents>.Subscribe(UIEvents.ReleasedAbilityCard, FindTiles);

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

            PickedTile(MouseToWorldView.HoverTileGridPos, attackModule.GetClosestTile(MouseToWorldView.HoverTileGridPos));

            Tooltip.HideTooltip_Static();
            GridStaticFunctions.ResetBattleTileColors();
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
        EventManager<UIEvents>.Unsubscribe(UIEvents.ReleasedAbilityCard, FindTiles);
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
        //EventManager<UIEvents, string>.Invoke(UIEvents.InfoTextUpdate, "Right mouse button to cancel action");

        if (movementModule.AccessableTiles.Contains(endPos)) {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Move);
            CurrentPath = movementModule.GetPath(endPos);
        }
        else if (attackModule.AttackableTiles.Contains(endPos)) {
            CurrentPath = movementModule.GetPath(attackModule.GetClosestTile(endPos));
            CurrentPath.Add(MouseToWorldView.HoverTileGridPos);

            Vector2Int calculatedLookDir = endPos - attackModule.GetClosestTile(endPos);
            calculatedLookDir.Clamp(new(-1, -1), new(1, 1));

            // Should not be here!
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Attack);
            if (UnitStaticManager.TryGetUnitFromGridPos(endPos, out var unit)) {
                int bonus = DamageManager.CaluculateDamage(this, unit, calculatedLookDir);
                int damage = Values.currentStats.Attack;

                if (damage + bonus > unit.UnitBaseData.BaseStatBlock.Defence)
                    Tooltip.ShowTooltip_Static($"{unit.UnitBaseData.Name} defense: {unit.UnitBaseData.BaseStatBlock.Defence} <br> base attack: <color=green>{damage}</color>  bonus: <color=green>+ {bonus}</color> = <b><color=green>{bonus + damage} (WIN) </color></b>");
                else
                    Tooltip.ShowTooltip_Static($"{unit.UnitBaseData.Name} defense {unit.UnitBaseData.BaseStatBlock.Defence} <br> base attack: <color=red>{damage}</color>  bonus: <color=red>+ {bonus}</color> = <b><color=red>{bonus + damage} (LOSE) </color></b>");
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

        Line.SetPosition(0, GridStaticFunctions.CalcWorldPos(gridPosition));
        for (int i = 1; i < CurrentPath.Count + 1; i++)
            Line.SetPosition(i, GridStaticFunctions.CalcWorldPos(CurrentPath[i - 1]));
    }
}
