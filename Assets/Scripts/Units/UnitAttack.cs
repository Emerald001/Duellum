using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "New UnitAttack", menuName = "Unit/UnitAttack")]
public class UnitAttack : ScriptableObject {
    public List<Effect> EffectsToApply;
    public Selector Selector;

    public AbilityType Type;
    public DamageType damageType;

    public int energyCost;
    public int Damage;
    [Range(0, 100)]
    public int RandomnessRange;

    public string Name;
    public string Description;
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
