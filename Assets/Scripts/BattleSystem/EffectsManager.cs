using System;

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
    public EffectType type;

    public bool canBeStacked;

    public int duration;
    public int sevarity;

    public Effect(EffectType type, bool canBeStacked, int duration, int sevarity) {
        this.type = type;
        this.canBeStacked = canBeStacked;
        this.duration = duration;
        this.sevarity = sevarity;
    }
}

public static class EffectsManager {
    public static void ApplyEffects(UnitValues data, Effect effect) {
        switch (effect.type) {
            case EffectType.AttackBoost:
                data.currentStats.Attack += effect.sevarity;
                break;
            case EffectType.DefenceBoost:
                data.currentStats.Defence += effect.sevarity;
                break;
            case EffectType.SpeedBoost:
                data.currentStats.Speed += effect.sevarity;
                break;
            case EffectType.Fury:

                break;
            case EffectType.Fear:

                break;
            case EffectType.Slow:
                data.currentStats.Speed -= effect.sevarity;
                break;
            case EffectType.Exhaust:
                data.currentStats.Speed -= 1000;
                break;
            case EffectType.KnockedOut:

                break;
        }
    }
}
