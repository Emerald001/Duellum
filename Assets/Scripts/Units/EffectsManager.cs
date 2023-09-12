using System;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType {
    AttackBoost,
    DefenceBoost,
    SpeedBoost,


    Burn,
    Poison,
    Slow,
    Drunk,
    Exhaust,
    KnockedOut,
}

[Serializable]
public class Effect {
    public Sprite Icon;
    public EffectType type;

    public bool canBeStacked;

    public int duration;
    public int sevarity;
}

public static class EffectsManager {
    public static void ApplyEffects(UnitValues data, List<Effect> effects) {
        foreach (var effect in effects) {
            switch (effect.type) {
                case EffectType.Poison:
                    data.currentStats.EnergyPoints -= 1 * effect.sevarity;
                    break;

                case EffectType.Burn:
                    // Apply Burn Effect
                    break;
            }
        }
    }
}
