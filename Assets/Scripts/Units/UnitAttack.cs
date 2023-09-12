using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New UnitAttack", menuName = "Unit/UnitAttack")]
public abstract class UnitAttack : ScriptableObject {
    public List<Effect> EffectsToApply;
    public Selector AreaOfEffectSelector;
    public Selector ApplicableTilesSelector;

    public AbilityType Type;
    public DamageType damageType;

    public int energyCost;
    public int Damage;
    [Range(0, 100)]
    public int RandomnessRange;
    public int cooldown;

    public string Name;
    public string Description;

    // Implementation
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();

    public int CurrentCooldown { get; set; }
}

public enum DamageType {
    Melee,
    Ranged,
    Fire,
    Poison,
    Frost,
    Life,
    Death,
}

public enum AbilityType {
    Damage,
    MapInteraction,
    Heal,
}
