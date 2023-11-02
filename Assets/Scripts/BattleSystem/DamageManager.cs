using UnityEngine;

public static class DamageManager
{
    public static void DealDamage(UnitController attackingUnit, params UnitController[] defendingUnits) {
        foreach (UnitController unit in defendingUnits) {
            EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitHit, unit);
            
            if (attackingUnit.Values.currentStats.Attack + CalculateDirectionalDamage(attackingUnit, unit) > unit.Values.currentStats.Defence) {
                unit.AddEffect(new Effect(
                    EffectType.KnockedOut,
                    false,
                    1000,
                    100));

                Debug.Log($"UNIT DIED");
                EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitDeath, unit);
                UnitStaticManager.UnitDeath(unit);
            }
        }
    }

    private static int CalculateDirectionalDamage(UnitController attackingUnit, UnitController defendingUnit) {
        Vector2Int mod = attackingUnit.LookDirection + defendingUnit.LookDirection;
        return Mathf.Max(Mathf.Abs(mod.x), Mathf.Abs(mod.y));
    }
}
