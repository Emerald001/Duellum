using UnityEngine;
using System.Collections.Generic;

public enum EffectType {
    Burn,
    Poison,
    Slow,
    Drunk,
    Exhaust,
    KnockedOut,
}

public class Effect {
    public Sprite Icon;
    public EffectType type;

    public bool canBeStacked;

    public int duration;
    public int sevarity;
}

public static class EffectsManager {
    public static void ApplyEffects(UnitData data, List<Effect> effects) {
        foreach (var item in effects) {
            switch (item.type) {
                case EffectType.Poison:
                    ApplyPoisonEffect(data, item.sevarity);
                    break;

                case EffectType.Burn:
                    // Apply Burn Effect
                    break;
            }
        }
    }

    private static void ApplyPoisonEffect(UnitData data, int severity) {
        data.BaseStatBlock.EnergyPoints -= 1 * severity;
    }
}