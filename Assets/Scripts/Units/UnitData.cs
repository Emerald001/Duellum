using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

[Serializable]
[CreateAssetMenu(fileName = "New UnitData", menuName = "Unit/UnitData")]
public class UnitData : ScriptableObject {
    [Header("Base Information")]
    public string Name;
    public string Description;
    [Range(1, 5)]
    public int Rank;

    public UnitRace Catagory;

    [Header("Visuals")]
    public Sprite Icon;
    public SpriteAtlas Animations;
    public GameObject PawnPrefab;
    public GameObject UnitCard;
    public float movementSpeed;

    [Header("Tile Coverage")]
    public UnitTileCoverType tileCover;
    public int TileCoverage;

    [Header("Stat Block")]
    public StatBlock BaseStatBlock;

    [Header("Attacks")]
    public UnitAttack Attack;

    [Header("Properties")]
    public List<UnitAttribute> Properties;
}

[Serializable]
public class StatBlock {
    public int Defence;
    public int Speed;
    public int Attack;

    public StatBlock(StatBlock clone) {
        Attack = clone.Attack;
        Defence = clone.Defence;
        Speed = clone.Speed;
    }
}

public enum UnitRace {
    Demon,
    Undead,
    Human,
    Beast,
    Elemental,
    Celestial,
}

public enum UnitTileCoverType {
    SingleTile,
    Line,
    Round,
}