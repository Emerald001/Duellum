using UnityEngine;

// TODO:
// Create a custom Drawer For this!
[CreateAssetMenu(menuName = "Abilty Card", fileName = "Card")]
public class AbilityCard : ScriptableObject
{
    [Header("Visuals")]
    public string Name;
    public string Discription;

    public Sprite Icon;
    public Sprite Background;
    public Sprite Border;

    [Header("Card Abilities")]
    public int ManaCost;

    [Header("Card Selectors")]
    public Selector areaOfEffectSelector;
    public Selector availabletilesSelector;

    public AbilityCardType abilityType;
    public Effect effectToApply;
    public Hex hexPrefab;
}

public enum AbilityCardType {
    ApplyEffect,
    PlaceBoulder,
    Revive,
    SkipOpponentsTurn,
}
