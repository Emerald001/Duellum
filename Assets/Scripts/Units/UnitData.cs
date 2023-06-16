using UnityEngine;
using UnityEngine.U2D;

[System.Serializable]
[CreateAssetMenu(fileName = "New UnitData", menuName = "Unit/UnitData")]
public class UnitData : ScriptableObject {
    [Header("Base Information")]
    public string Name;
    public string Description;
    public UnitCatagory Catagory;
    public int Rank;

    [Header("Visuals")]
    public Sprite Icon;
    public SpriteAtlas animations;
    public GameObject Pawn;

    [Header("Tile Coverage")]
    public UnitTileCoverType tileCover;
    public int TileCoverage;

    [Header("Stat Block")]
    public StatBlock statBlock;

    [Header("Base Attack")]
    public DamageType damageType;
    public Selector selector;
    public int Damage;
    //public List<Effects> Effects;
    //public List<> Bonuses;

    [Header("Abilities")]
    //public List<Abilities> Abilities;

    [Header("Properties")]
    //public List<Property> Properties;

    // Hidden
    [HideInInspector] public StatBlock CurrentStats;
    //[HideInInspector] public List<Property> Properties;
    //[HideInInspector] public List<Effect> Properties;
    [HideInInspector] public int Morale;
}

[System.Serializable]
public class StatBlock {
    public int Defence;
    public int Initiative;
    public int Speed;
    public int Critical;
}

public enum UnitCatagory {

}

public enum UnitTileCoverType {

}

public enum DamageType {
    Fire,
    Frost,
    Life,
    Death,
}

[CreateAssetMenu(fileName = "New UnitAbility", menuName = "Unit/UnitAbility")]
public class UnitAbility : ScriptableObject {
    public AbilityType Type;
    //public List<Effect> Effects;
    public Selector Selector;
    public int Damage;
}

public enum AbilityType {
    Damage,
    MapInteraction,
    Heal,
}