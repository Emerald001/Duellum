using System;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType {
    AttackBoost,
    DefenceBoost,
    SpeedBoost,
    Fury,
    Fear,
    Slow,
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
                case EffectType.AttackBoost:
                    break;
                case EffectType.DefenceBoost:
                    break;
                case EffectType.SpeedBoost:
                    break;
                case EffectType.Fury:
                    break;
                case EffectType.Fear:
                    break;
                case EffectType.Slow:
                    break;
                case EffectType.Exhaust:
                    break;
                case EffectType.KnockedOut:
                    break;
            }
        }
    }
}
