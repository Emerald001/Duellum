using UnityEngine;

public static class DamageManager
{
    public static void DealDamage(UnitValues attackingUnit, params UnitController[] defendingUnits) {
        foreach (UnitController unit in defendingUnits) {
            if (attackingUnit.currentStats.Attack + RollDice() > unit.Values.currentStats.Defence) {
                unit.AddEffect(new Effect(
                    EffectType.KnockedOut,
                    false,
                    1000,
                    100));

                Debug.Log($"UNIT DIED");

                UnitStaticManager.UnitDeath(unit);
            }
        }
    }

    private static int RollDice() {
        int roll = Random.Range(1, 7);
        Debug.Log($"Rolled a {roll}");
        return roll;
    }
}
