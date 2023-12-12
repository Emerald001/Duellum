using System;
using System.Collections.Generic;

public static class EnemyAIActionEvaluator {
    public static Tuple<int, List<Action>> CalculateActionValue(UnitController unit) {
        if (unit == null) 
            return new(0, null);

        //int result = 0;

        CalculateAttackValue();
        CalculateMovementValue();

        return new(0, null);
    }

    private static int CalculateAttackValue() {
        // Can unit attack (check attackable tiles)
        // Can unit kill other enemy?
        // if yes
            // Grab Best unit to attack (grab unit with the highest damage stat)
        // If no
            // Do not attack 

        return 0;
    }

    private static int CalculateMovementValue() {
        // Given Tile from attack
        // Can we get there while also getting a Card?
        // What is the best position to stand after movement?
            // No Flanking?
            // What positions can the unit flank in?
            // Farthest away from enemies? (ranged)
            // Behind cover


        return 0;
    }
}

//public enum Difficulty {
//    Defeat = 1,
//    Win = 3,
//    Survive = 5,
//    Raging = 10,
//}
