using System.Collections.Generic;
using UnityEngine;

// TODO:
// Create a custom Drawer For this!
[CreateAssetMenu(menuName = "Ability Card", fileName = "Card")]
public class AbilityCard : Card {
    [Header("Card Selectors")]
    public List<CardState> cardStates = new();

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
    [Space(10)]
    public int Damage;
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
