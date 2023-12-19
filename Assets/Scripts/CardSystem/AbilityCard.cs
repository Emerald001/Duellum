using UnityEngine;

// TODO:
// Create a custom Drawer For this!
[CreateAssetMenu(menuName = "Abilty Card", fileName = "Card")]
public class AbilityCard : ScriptableObject {
    [Header("Visuals")]
    public string Name;
    public string ShortcutName;
    public string Discription;

    public Sprite Icon;
    public Sprite Background;
    public Sprite Border;

    [Header("Card Abilities")]
    public int ManaCost;

    [Header("Card Selectors")]
    public Selector areaOfEffectSelector;
    public Selector availabletilesSelector;

    [Header("Extra Data")]
    public AbilityCardType abilityType;
    [Space(10)]
    public Effect effectToApply;
    [Space(10)]
    public Tile hexPrefab;
    [Space(10)]
    public TileEffect tileEffect;
}

public enum AbilityCardType {
    ApplyEffect,
    PlaceBoulder,
    Revive,
    SkipOpponentsTurn,
    MoveUnit,
    SpinUnit,
    Summon,
    ApplyTileEffect,
    SmokeBomb,
    Grapple,
    Charm,
    AreaOfEffectAttack,
}