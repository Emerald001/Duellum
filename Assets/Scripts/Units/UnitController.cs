using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitController : MonoBehaviour {
    public UnitData UnitBaseData { get; private set; }

    private UnitValues values;
    private UnitMovementComponent unitMovementComponent;

    public void SetUp(UnitData data) {
        UnitBaseData = Instantiate(data);

        values = new(UnitBaseData);
    }
}

public class UnitValues {
    public UnitValues(UnitData data) {
        currentStats = new(data.BaseStatBlock);
    }

    [HideInInspector] public int Morale;
    public StatBlock currentStats;
    public List<Effect> CurrentEffects;

    public void AddEffect(Effect effect) {
        if (effect.canBeStacked)
            CurrentEffects.Add(effect);
        else if (!CurrentEffects.Select(x => x.type).Contains(effect.type))
            CurrentEffects.Add(effect);
    }
}

public class UnitMovementComponent {
    public bool IsDone { get; private set; }

    private readonly ActionQueue movementQueue;
    private Dictionary<Vector2Int, Vector2Int> parentDictionary;
    private List<Vector2Int> currentAccessableTiles;

    public UnitMovementComponent(UnitData data) {
        movementQueue = new(() => IsDone = true);
    }

    public void FindAccessibleTiles(Vector2Int gridPos, int speedValue) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new(); // Use HashSet for faster membership checks

        openList.Add(gridPos);

        for (int i = 0; i < speedValue; i++) {
            foreach (var currentPos in openList.ToList()) { // Use ToList() to avoid modification during iteration
                GridStaticFunctions.RippleThroughSquareGridPositions(currentPos, 2, (neighbour, count) =>
                {
                    if (neighbour == currentPos)
                        return;

                    if (GridStaticFunctions.TryGetUnitFromGridPos(neighbour, out var tmp))
                        return;

                    if (!GridStaticFunctions.Grid[neighbour].CompareTag("WalkableTile") ||
                        openList.Contains(neighbour) ||
                        closedList.Contains(neighbour) ||
                        layerList.Contains(neighbour))
                        return;

                    layerList.Add(neighbour);
                    currentAccessableTiles.Add(neighbour);
                    parentDictionary[neighbour] = currentPos;
                });

                closedList.Add(currentPos);
            }

            openList.Clear();
            openList.AddRange(layerList); // Use ForEach for adding elements from one list to another
            layerList.Clear();
        }
    }

    private void Move() {
        IsDone = false;
    }
}