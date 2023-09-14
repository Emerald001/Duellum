using System.Collections.Generic;
using UnityEngine;

public static class AbilityManager {
    public static void PerformAbility(AbilityCard card, params Vector2Int[] positions) {
        switch (card.abilityType) {
            case AbilityCardType.ApplyEffect:
                List<UnitController> controllerList = new();
                foreach (Vector2Int position in positions) {
                    if (GridStaticFunctions.TryGetUnitFromGridPos(position, out var unit))
                        controllerList.Add(unit);
                }

                foreach (var unit in controllerList)
                    unit.AddEffect(card.effectToApply);
            break;

            case AbilityCardType.Teleport:
                // Select any unit
            break;

            case AbilityCardType.PlaceBoulder:
                // Select a specific tile
            break;

            case AbilityCardType.Revive:
                // Select any unit
            break;

            case AbilityCardType.SkipOpponentsTurn:
                // Select All Enemy units
            break;

            default:
                throw new System.NotImplementedException($"{card.abilityType} Not Yet Implemented");
        }
    }
}
