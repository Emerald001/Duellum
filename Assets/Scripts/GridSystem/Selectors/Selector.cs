using System;

[Serializable]
public class Selector {
    public SelectorType type;

    public bool includeCentreTile;
    public bool AllDirections;

    public int range = 1;
    public int rotIndex = 1;
}

public enum SelectorType {
    SingleTile,
    Line,
    Circle,
    AllUnits,
    FriendlyUnits,
    EnemyUnits,
}
