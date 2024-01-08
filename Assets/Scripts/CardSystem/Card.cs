using UnityEngine;

public abstract class Card : ScriptableObject {
    [Header("Visuals")]
    public string Name;
    public string ShortcutName;
    public string Discription;
    public CardType CardType;

    public Sprite Icon;
    public Sprite Background;
    public Sprite Border;
}

public enum CardType { 
    UnitSpecific = 0,
    Effect = 1,
    Action = 2,
    Dungeon = 3,
}