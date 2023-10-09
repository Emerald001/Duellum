using System.Collections.Generic;
using UnityEngine;

public static class AbilityManager {
    public static void PerformAbility(AbilityCard card, params Vector2Int[] positions) {
        List<UnitController> controllerList = new();
        foreach (Vector2Int position in positions) {
            if (UnitStaticManager.TryGetUnitFromGridPos(position, out var unit))
                controllerList.Add(unit);
        }

        switch (card.abilityType) {
            case AbilityCardType.ApplyEffect:
                foreach (var unit in controllerList)
                    unit.AddEffect(card.effectToApply);
            break;

            case AbilityCardType.PlaceBoulder:
                GridStaticFunctions.ReplaceHex(card.hexPrefab, positions);
            break;

            case AbilityCardType.Revive:
                foreach (var unit in controllerList)
                    unit.Values.RemoveEffect(card.effectToApply.type);
                break;

            case AbilityCardType.SkipOpponentsTurn:
                // Select All Enemy units
            break;

            default:
                throw new System.NotImplementedException($"{card.abilityType} Not Yet Implemented");
        }
    }
}
