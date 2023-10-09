using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTurnController : TurnController {
    public override void OnEnter() {
        base.OnEnter();

        units = UnitStaticManager.PlayerUnitsInPlay;

        HighlightUnits();
        isPicking = true;

        EventManager<BattleEvents>.Subscribe(BattleEvents.ReleasedAbilityCard, HighlightUnits);
    }

    public override void OnUpdate() {
        if (isPicking) {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                PickUnit(MouseToWorldView.HoverTileGridPos);
            return;
        }

        currentUnit.OnUpdate();
        if (currentUnit.IsDone)
            IsDone = true;

        if (currentUnit.HasPerformedAction)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            EventManager<BattleEvents>.Subscribe(BattleEvents.ReleasedAbilityCard, HighlightUnits);

            currentUnit.OnExit();
            currentUnit = null;
            isPicking = true;

            HighlightUnits();
        }
    }

    private void HighlightUnits() {
        GridStaticFunctions.ResetTileColors();

        List<Vector2Int> positions = units.Select(unit => UnitStaticManager.UnitPositions[unit]).ToList();
        GridStaticFunctions.HighlightTiles(positions, HighlightType.OwnPositionHighlight);
    }

    protected override void PickUnit(Vector2Int unitPosition) {
        base.PickUnit(unitPosition);

        if (!isPicking)
            EventManager<BattleEvents>.Unsubscribe(BattleEvents.ReleasedAbilityCard, HighlightUnits);
    }

    public void AddUnit(UnitController unit) {
        if (!units.Contains(unit))
            units.Add(unit);
    }

    public void RemoveUnit(UnitController unit) {
        if (units.Contains(unit))
            units.Remove(unit);
    }
}
