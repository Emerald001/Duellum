using System.Collections.Generic;
using UnityEngine;

public class RippleEffect : MonoBehaviour {
    [SerializeField] private int rippleRange;
    [SerializeField] private float rippleStrength;

    private readonly Vector2Int[] evenNeighbours = {
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
        };

    private readonly Vector2Int[] unevenNeighbours = {
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse0) && MouseToWorldView.HoverTileGridPos != Vector2Int.zero)
            Ripple(MouseToWorldView.HoverTileGridPos);
    }

    public void Ripple(Vector2Int gridPos) {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        openList.Add(gridPos);
        for (int i = 0; i < rippleRange; i++) {
            for (int j = 0; j < openList.Count; j++) {
                Vector2Int currentPos = openList[j];
                Vector2Int[] listToUse;

                if (currentPos.y % 2 != 0)
                    listToUse = unevenNeighbours;
                else
                    listToUse = evenNeighbours;

                for (int k = 0; k < 6; k++) {
                    Vector2Int neighbour = currentPos + listToUse[k];
                    if (openList.Contains(neighbour) || closedList.Contains(neighbour) || layerList.Contains(neighbour) || !UnitStaticFunctions.Grid.ContainsKey(neighbour))
                        continue;

                    layerList.Add(neighbour);
                }

                Hex currentHex = UnitStaticFunctions.Grid[currentPos];
                currentHex.ClearQueue();
                List<Action> queue = new() {
                    new WaitAction(Mathf.Pow(i, i / 70f) - Mathf.Pow(1, 1 / 70f)),
                    new MoveObjectAction(currentHex.gameObject, 50 / Mathf.Pow(i, i / 70f), currentHex.StandardPosition - new Vector3(0, rippleRange / rippleStrength / Mathf.Pow(i, i / 10f), 0)),
                    new MoveObjectAction(currentHex.gameObject, 2 / Mathf.Pow(i, i / 70f), currentHex.StandardPosition),
                };
                currentHex.SetActionQueue(queue);

                closedList.Add(openList[j]);
            }

            openList.Clear();
            for (int j = 0; j < layerList.Count; j++)
                openList.Add(layerList[j]);

            layerList.Clear();
        }
    }
}
