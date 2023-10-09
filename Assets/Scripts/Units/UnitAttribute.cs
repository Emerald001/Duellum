using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New UnitAttribute", menuName = "Unit/UnitAttribute")]
public class UnitAttribute : ScriptableObject {
    public string Name;
    public string Description;

    public AttributeType type;

    // Resistance, weakness and Immunity
    public List<RWIData> RWIDatas;

    // Effect Immunity
    public List<EffectType> ImmuneEffects;
}

public enum AttributeType {
    Resistance,
    Weakness,
    DamageImmunity,
    EffectImmunity,
    ExtraDamage,
}

[Serializable]
public struct RWIData {
    public DamageType DamageType;
    public int Severity;
}

public static class AttributeManager {

}