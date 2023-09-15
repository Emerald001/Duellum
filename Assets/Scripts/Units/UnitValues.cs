using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitValues {
    public UnitValues(UnitData data) {
        currentStats = new(data.BaseStatBlock);
        baseData = data;

        EventManager<BattleEvents>.Subscribe(BattleEvents.NewTurn, ApplyEffects);
    }

    [HideInInspector] public int Morale;

    public StatBlock currentStats;
    public List<Effect> CurrentEffects = new();
    
    private UnitData baseData;

    public void AddEffect(Effect effect) {
        if (effect.canBeStacked)
            CurrentEffects.Add(effect);
        else if (!CurrentEffects.Select(x => x.type).Contains(effect.type))
            CurrentEffects.Add(effect);
        else
            CurrentEffects.First(x => x.type == effect.type).duration = Mathf.Max(CurrentEffects.First(x => x.type == effect.type).duration, effect.duration);
    }

    private void ApplyEffects() {
        currentStats = new(baseData.BaseStatBlock);
        EffectsManager.ApplyEffects(this, CurrentEffects);
    }
}
