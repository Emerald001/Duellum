using System;

[Serializable]
public class Selector {
    public SelectorType type;
    public bool isHex;

    public bool includeCentreTile;
    public bool AllDirections;

    public int range = 1;
    public int rotIndex = 1;

    // For Alltiles
    public bool includeWater;
    public bool includeCover;
    public bool excludeUnits;
}

public enum SelectorType {
    SingleTile,
    Line,
    Circle,
    AllUnits,
    FriendlyUnits,
    EnemyUnits,
    AllTiles,
}
