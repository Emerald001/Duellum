using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New UnitAttack", menuName = "Unit/UnitAttack")]
public class UnitAttack : ScriptableObject {
    public string Name;
    public string Description;

    public List<Effect> EffectsToApply;
    //public Selector AreaOfEffectSelector;
    public Selector ApplicableTilesSelector;

    //public AbilityType Type;
    public DamageType damageType;

    //public int energyCost;
    //public int Damage;
    //[Range(0, 100)]
    //public int RandomnessRange;
    //public int cooldown;
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
