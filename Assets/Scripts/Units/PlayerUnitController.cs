using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : UnitController {
    public LineRenderer Line { get; set; }

    private readonly List<Vector2Int> lastAbilityTiles = new();
    private List<Vector2Int> lastHighlightedTiles = new();

    private List<Vector2Int> CurrentPath = new();

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
        if (Line != null)
            CreatePathForLine();

        base.OnUpdate();

        RunAttack();
    }

    private void RunAttack() {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            PickedTile(
                MouseToWorldView.HoverTileGridPos, 
                attackModule.GetClosestTile(MouseToWorldView.HoverTileGridPos, gridPosition, MouseToWorldView.HoverPointPos, unitMovement.AccessableTiles)
            );
    }

    public override void OnExit() {
        base.OnExit();

        Line.enabled = false;

        EventManager<BattleEvents>.Unsubscribe(BattleEvents.ReleasedAbilityCard, FindTiles);
    }

    public void HighlightTiles() {
        GridStaticFunctions.HighlightTiles(unitMovement.AccessableTiles, HighlightType.MovementHighlight);
        GridStaticFunctions.HighlightTiles(attackModule.AttackableTiles, HighlightType.AttackHighlight);
        GridStaticFunctions.Grid[gridPosition].SetHighlight(HighlightType.OwnPositionHighlight);
    }

    public void ResetTiles() {
        if (lastAbilityTiles.Count > 0)
            ChangeHexColor(lastAbilityTiles, HighlightType.None);
        lastAbilityTiles.Clear();

        foreach (var pos in lastHighlightedTiles)
            GridStaticFunctions.Grid[pos].SetHighlight(HighlightType.None);

        GridStaticFunctions.Grid[gridPosition].SetHighlight(HighlightType.None);
    }

    public override void FindTiles() {
        base.FindTiles();

        HighlightTiles();
    }

    public void ChangeHexColor(List<Vector2Int> list, HighlightType type) {
        for (int i = 0; i < list.Count; i++)
            GridStaticFunctions.Grid[list[i]].SetHighlight(type);
    }

    private void CreatePathForLine() {
        Vector2Int endPos = MouseToWorldView.HoverTileGridPos;

        if (unitMovement.AccessableTiles.Contains(endPos))
            CurrentPath = unitMovement.GetPath(endPos);
        else if (attackModule.AttackableTiles.Contains(endPos)) {
            CurrentPath = unitMovement.GetPath(attackModule.GetClosestTile(endPos, gridPosition, MouseToWorldView.HoverPointPos, unitMovement.AccessableTiles));
            CurrentPath.Add(MouseToWorldView.HoverTileGridPos);
        }
        else {
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
