using UnityEditor.Rendering.LookDev;
using UnityEngine;

public static class DamageManager {
    public static void DealDamage(UnitController attackingUnit, params UnitController[] defendingUnits) {
        foreach (UnitController unit in defendingUnits) {
            EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitHit, unit);

            if (CaluculateDamage(attackingUnit, unit) > unit.Values.currentStats.Defence) {
                unit.AddEffect(new Effect(
                    EffectType.KnockedOut,
                    false,
                    1000,
                    100));

                EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitDeath, unit);
                UnitStaticManager.UnitDeath(unit);
            }
        }
    }

    public static int CaluculateDamage(UnitController attackingUnit, UnitController defendingUnit, Vector2Int lookdir = new()) {
        if (lookdir == Vector2Int.zero) 
            return attackingUnit.Values.currentStats.Attack + CalculateDirectionalDamage(attackingUnit.LookDirection, defendingUnit);

        return CalculateDirectionalDamage(lookdir, defendingUnit);
    }
    
    public static int CalculateDirectionalDamage(Vector2Int attackingLookDir, UnitController defendingUnit) {
        Vector2Int mod = attackingLookDir + defendingUnit.LookDirection;
        return Mathf.Max(Mathf.Abs(mod.x), Mathf.Abs(mod.y)) + 1;
    }
}
