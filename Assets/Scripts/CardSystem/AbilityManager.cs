using UnityEngine;

public static class AbilityManager {
    public static void PerformAbility(AbilityCardType type, params Vector2Int[] positions) {
        switch (type) {
            case AbilityCardType.AttackBoost:
                // Select any unit
                break;
            case AbilityCardType.DefenceBoost:
                // Select any unit
                break;
            case AbilityCardType.SpeedBoost:
                // Select any unit
                break;
            case AbilityCardType.Climb:
                // Select any unit
                break;
            case AbilityCardType.Teleport:
                // Select any unit
                break;
            case AbilityCardType.Trap:
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
            case AbilityCardType.FuryEffect:
                // Select any unit
                break;
            case AbilityCardType.FearEffect:
                // Select any unit
                break;

            default:
                throw new System.NotImplementedException($"{type} Not Yet Implemented");
        }
    }
}