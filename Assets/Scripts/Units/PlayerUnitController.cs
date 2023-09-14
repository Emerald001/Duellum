using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : UnitController {
    public LineRenderer Line { get; set; }

    private Vector2Int lastHoverPos = Vector2Int.zero;
    private readonly List<Vector2Int> lastAbilityTiles = new();
    private List<Vector2Int> lastHighlightedTiles = new();

    private List<Vector2Int> CurrentPath = new();

    public override void OnEnter() {
        Line = GetComponent<LineRenderer>();
        //turnManager.UIManager.ActivateButtons();

        base.OnEnter();
    }

    public override void OnUpdate() {
        if (Line != null)
            CreatePathForLine();

        base.OnUpdate();

        if (attackModule.PickedAttack) {
            RunAbility();
            return;
        }

        RunAttack();
    }

    private void RunAttack() {
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            PickedTile(MouseToWorldView.HoverTileGridPos, MouseToWorldView.HoverTileGridPos);
            //Tooltip.HideTooltip_Static();
        }

        // TODO:
        // GET TOOLTIP WORKING!
        //if (AttackableTiles.Contains(MouseValues.HoverTileGridPos)) {
        //    List<HealthComponent> list = new List<HealthComponent> {
        //        EnemyPositions[MouseValues.HoverTileGridPos].HealthComponent
        //    };

        //    Tooltip.ShowTooltip_Static(GetEnemyInfo(list, values.damageValue));
        //    lastHoverPos = MouseValues.HoverTileGridPos;
        //}

        //if (lastHoverPos != MouseValues.HoverTileGridPos && lastHoverPos != Vector2Int.zero) {
        //    Tooltip.HideTooltip_Static();
        //    lastHoverPos = Vector2Int.zero;
        //}
    }

    private void RunAbility() {
    //    List<Vector2Int> highlightedPositions = new List<Vector2Int>();

    //    foreach (var pos in lastHighlightedTiles)
    //        if (!highlightedPositions.Contains(pos)) {
    //            Hex hex = turnManager.Tiles[pos].GetComponent<Hex>();
    //            hex.ResetColor();
    //        }

    //    if (pickedAbility.HitDiameter >= 1 && pickedAbility.Tiles.Contains(MouseValues.HoverTileGridPos)) {
    //        highlightedPositions = DefineMultipleTiles.GetTiles(MouseValues.HoverTileGridPos, pickedAbility.HitDiameter, turnManager.Tiles);
    //        foreach (var pos in highlightedPositions) {
    //            Hex hex = turnManager.Tiles[pos].GetComponent<Hex>();
    //            hex.SetColor(turnManager.BattleSettings.SelectedTileColor);
    //        }
    //        lastHighlightedTiles = highlightedPositions;
    //    }
    //    else
    //        highlightedPositions.Add(MouseValues.HoverTileGridPos);

    //    if (pickedAbility.Tiles.Contains(MouseValues.HoverTileGridPos)) {
    //        List<HealthComponent> list = new();

    //        foreach (var target in highlightedPositions)
    //            if (pickedAbility.AbilityApplicable.ContainsKey(target))
    //                list.Add(pickedAbility.AbilityApplicable[target].HealthComponent);

    //        if (pickedAbility.ToolTipText(list) == null)
    //            Tooltip.HideTooltip_Static();
    //        else
    //            Tooltip.ShowTooltip_Static(pickedAbility.ToolTipText(list));

    //        lastHoverPos = MouseValues.HoverTileGridPos;
    //    }
    //    if (lastHoverPos != MouseValues.HoverTileGridPos && lastHoverPos != Vector2Int.zero) {
    //        Tooltip.HideTooltip_Static();
    //        lastHoverPos = Vector2Int.zero;
    //    }

    //    if (Input.GetKeyDown(KeyCode.Mouse0)) {
    //        if (pickedAbility.Definer != null)
    //            pickedAbility.PickedTile(highlightedPositions.ToArray(), pickedAbility.Definer.GetClosestTile(gridPos, MouseValues.HoverTileGridPos, MouseValues.HoverPointPos, AccessableTiles));
    //        else
    //            pickedAbility.PickedTile(highlightedPositions.ToArray(), gridPos);
    //        Tooltip.HideTooltip_Static();
    //    }
    }

    public override void OnExit() {
        base.OnExit();

        //turnManager.UIManager.DeactivateButtons();
        //Tooltip.HideTooltip_Static();
        Line.enabled = false;
    }

    public void ResetTiles() {
        //ResetHexColor(AccessableTiles);
        //ResetHexColor(AttackableTiles);

        if (lastAbilityTiles.Count > 0)
            ChangeHexColor(lastAbilityTiles, HighlightType.None);
        lastAbilityTiles.Clear();

        foreach (var pos in lastHighlightedTiles)
            GridStaticFunctions.Grid[pos].SetHighlight(HighlightType.None);

        GridStaticFunctions.Grid[gridPosition].SetHighlight(HighlightType.None);
    }

    public override void FindTiles() {
        //Hella cursed, shall be made better soon
        //turnManager.UIManager.SetAbilities(abilities, this);

        base.FindTiles();

        //ChangeHexColor(unitMovement.AccessableTiles, turnManager.BattleSettings.WalkableTileColor);
        //ChangeHexColor(AttackableTiles, turnManager.BattleSettings.AttackableTileColor);

        // TODO:
        // Give this a highlighted color
        GridStaticFunctions.Grid[gridPosition].SetHighlight(HighlightType.None);
    }

    public void ChangeHexColor(List<Vector2Int> list, HighlightType type) {
        for (int i = 0; i < list.Count; i++)
            GridStaticFunctions.Grid[list[i]].SetHighlight(type);
    }

    private void CreatePathForLine() {
        var endPos = MouseToWorldView.HoverTileGridPos;

        //if (pickedAttack != null) {
        //    if (pickedAttack.Tiles.Contains(endPos) && pickedAbility.Definer != null) {
        //        CurrentPath = pathfinding.FindPathToTile(gridPos, pickedAbility.Definer.GetClosestTile(gridPos, endPos, MouseValues.HoverPointPos, AccessableTiles), TileParents);
        //        CurrentPath.Add(MouseValues.HoverTileGridPos);
        //    }
        //    else {
        //        Line.enabled = false;
        //        return;
        //    }
        //}

        if (unitMovement.AccessableTiles.Contains(endPos))
            CurrentPath = unitMovement.GetPath(endPos);
        //else if (AttackableTiles.Contains(endPos)) {
        //    CurrentPath = pathfinding.FindPathToTile(gridPos, defineAttackableTiles.GetClosestTile(gridPos, endPos, MouseValues.HoverPointPos, AccessableTiles), TileParents);
        //    CurrentPath.Add(MouseValues.HoverTileGridPos);
        //}
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

    public void SelectAttack(int index) {
        //base.SelectAbility(index);

        //Line.enabled = false;

        //if (pickedAbility != null) {
        //    if (pickedAbility.Ranged || pickedAbility.DropAnywhere || pickedAbility.DropOnEmptyTile)
        //        ResetHexColor(AccessableTiles);
        //    ResetHexColor(AttackableTiles);

        //    if (lastAbilityTiles.Count > 0)
        //        ResetHexColor(lastAbilityTiles);

        //    ChangeHexColor(pickedAbility.Tiles, turnManager.BattleSettings.AttackableTileColor);

        //    foreach (var tile in pickedAbility.Tiles)
        //        lastAbilityTiles.Add(tile);

        //    turnManager.Tiles[gridPos].GetComponent<Hex>().GivenColor = turnManager.BattleSettings.ActiveUnitTileColor;
        //    turnManager.Tiles[gridPos].GetComponent<Hex>().SetColor(turnManager.BattleSettings.ActiveUnitTileColor);
        //}
        //if (pickedAbility == null) {
        //    ResetHexColor(lastAbilityTiles);
        //    lastAbilityTiles.Clear();

        //    turnManager.Tiles[gridPos].GetComponent<Hex>().GivenColor = turnManager.BattleSettings.ActiveUnitTileColor;
        //    turnManager.Tiles[gridPos].GetComponent<Hex>().SetColor(turnManager.BattleSettings.ActiveUnitTileColor);

        //    ChangeHexColor(AccessableTiles, turnManager.BattleSettings.WalkableTileColor);
        //    ChangeHexColor(AttackableTiles, turnManager.BattleSettings.AttackableTileColor);
        //}
    }

    //private string GetEnemyInfo(List<HealthComponent> enemyHealthComponents, int DamageValue) {
    //    string kills = "";

    //    for (int i = 0; i < enemyHealthComponents.Count; i++) {
    //        var thisString = enemyHealthComponents[i].Owner.gameObject.name + "\n";
    //        Vector2Int minmax = enemyHealthComponents[i].CalcDamage(DamageValue);

    //        thisString += "Damage " + minmax.x + "-" + minmax.y + "\n";

    //        var MinKills = minmax.x > enemyHealthComponents[i].Health ? 1 : 0;
    //        var MaxKills = minmax.y < enemyHealthComponents[i].Health ? 0 : 1;

    //        if (MinKills != MaxKills)
    //            thisString += "Kills " + MinKills + "-" + MaxKills;
    //        else
    //            thisString += "Kills " + MinKills;

    //        if (i < enemyHealthComponents.Count - 1)
    //            thisString += "\n";

    //        kills += thisString;
    //    }

    //    return kills;
    //}
}
