using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitValues {
    public UnitValues(UnitData data) {
        currentStats = new(data.BaseStatBlock);
        baseData = data;

        EventManager<BattleEvents>.Subscribe(BattleEvents.NewTurn, ApplyEffects);
        EventManager<BattleEvents>.Subscribe(BattleEvents.NewTurn, LowerEffectCooldowns);
    }

    [HideInInspector] public int Morale;

    public StatBlock currentStats;
    public List<Effect> CurrentEffects = new();
    
    private readonly UnitData baseData;

    public void AddEffect(Effect effect) {
        if (effect.canBeStacked)
            CurrentEffects.Add(effect);
        else if (!CurrentEffects.Select(x => x.type).Contains(effect.type))
            CurrentEffects.Add(effect);
        else
            CurrentEffects.First(x => x.type == effect.type).duration = Mathf.Max(CurrentEffects.First(x => x.type == effect.type).duration, effect.duration);

        EffectsManager.ApplyEffects(this, effect);
    }

    public void RemoveEffect(EffectType type) {
        if (CurrentEffects.Count == 0) 
            return;

        IEnumerable<Effect> tmp = CurrentEffects.Where(x => x.type == type);
        foreach (var item in tmp)
            CurrentEffects.Remove(item);
    }

    private void ApplyEffects() {
        currentStats = new(baseData.BaseStatBlock);

        foreach (var item in CurrentEffects)
            EffectsManager.ApplyEffects(this, item);
    }

    private void LowerEffectCooldowns() {
        for (int i = CurrentEffects.Count - 1; i >= 0; i--) {
            Effect effect = CurrentEffects[i];
            if (effect.duration > 0)
                effect.duration--;
            else
                CurrentEffects.RemoveAt(i);
        }
    }
}
