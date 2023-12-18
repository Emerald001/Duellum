using System.Collections.Generic;
using UnityEngine;

public class GridEffects : MonoBehaviour {
    [Header("Raise")]
    [SerializeField] Selector RaiseSelector;
    [SerializeField] private float height;

    [Header("Ripple")]
    [SerializeField] Selector RippleSelector;
    [SerializeField] private float rippleStrength;

    private void Update() {
        if (MouseToWorldView.HoverTileGridPos == GridStaticFunctions.CONST_EMPTY)
            return;

        if (Input.GetKeyDown(KeyCode.K))
            Ripple(MouseToWorldView.HoverTileGridPos, rippleStrength);
        if (Input.GetKeyDown(KeyCode.Mouse1))
            Raise(MouseToWorldView.HoverTileGridPos, Input.GetKey(KeyCode.LeftShift), height);
    }

    private void Ripple(Vector2Int gridPos, float rippleStrength) {
        GridStaticFunctions.RippleThroughGridPositions(gridPos, RippleSelector.range, (gridPos, i) => {
            Tile currentHex = GridStaticFunctions.Grid[gridPos];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                    new WaitAction(i / 50f),
                    new MoveObjectAction(currentHex.gameObject, 30, currentHex.StandardWorldPosition - new Vector3(0, rippleStrength, 0)),
                    new MoveObjectAction(currentHex.gameObject, 20, currentHex.StandardWorldPosition + new Vector3(0, rippleStrength / 3, 0)),
                    new MoveObjectAction(currentHex.gameObject, 10, currentHex.StandardWorldPosition - new Vector3(0, rippleStrength / 6, 0)),
                    new MoveObjectAction(currentHex.gameObject, 5, currentHex.StandardWorldPosition + new Vector3(0, rippleStrength / 15, 0)),
                    new MoveObjectAction(currentHex.gameObject, 2, currentHex.StandardWorldPosition),
                };
            currentHex.SetActionQueue(queue);
        });

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .4f);
    }

    private void Raise(Vector2Int gridPos, bool invert, float height) {
        List<Vector2Int> positions = GridStaticSelectors.GetPositions(RaiseSelector, gridPos, 0);

        for (int i = 0; i < positions.Count; i++) {
            float newHeight = invert ? -height : height;
            Tile currentHex = GridStaticFunctions.Grid[positions[i]];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                    new MoveObjectAction(currentHex.gameObject, 20, currentHex.StandardWorldPosition + new Vector3(0, newHeight, 0)),
                    new DoMethodAction(() => currentHex.StandardWorldPosition = currentHex.transform.position),
                };
            currentHex.SetActionQueue(queue);
        }

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .2f);
    }
}

public enum GridEffectsEvents {
    DO_RIPPLE,
    RAISE_TILE,
    LOWER_TILE,
}