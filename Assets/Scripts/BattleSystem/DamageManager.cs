using UnityEngine;

public static class DamageManager {
    public static void DealDamage(UnitController attackingUnit, params UnitController[] defendingUnits) {
        EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, BattleLogString(attackingUnit, "Attacked", defendingUnits));
        
        foreach (UnitController unit in defendingUnits) {
            EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitHit, unit);

            if (CaluculateDamage(attackingUnit, unit) > unit.Values.currentStats.Defence) {
                unit.AddEffect(new Effect(
                    EffectType.KnockedOut,
                    false,
                    1000,
                    100));

                EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitDeath, unit);
                EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, BattleLogString(attackingUnit, "Killed", unit));
                UnitStaticManager.UnitDeath(unit);
            }
        }
    }

    public static void DealDamage(int flatAttack, params UnitController[] defendingUnits) {
        //EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, BattleLogString(attackingUnit, "Attacked", defendingUnits));

        foreach (UnitController unit in defendingUnits) {
            EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitHit, unit);

            if (flatAttack > unit.Values.currentStats.Defence) {
                unit.AddEffect(new Effect(
                    EffectType.KnockedOut,
                    false,
                    1000,
                    100));

                EventManager<BattleEvents, UnitController>.Invoke(BattleEvents.UnitDeath, unit);
                //EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, BattleLogString(attackingUnit, "Killed", unit));
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

    private static string BattleLogString(UnitController attackingUnit, string keyword, params UnitController[] defendingUnits) {
        string result = "";

        result += $"<color=red>{attackingUnit.UnitBaseData.Name}</color> {keyword} ";
        foreach (var item in defendingUnits)
            result += $"<color=green>{item.UnitBaseData.Name}</color>";

        return result;
    }
}
