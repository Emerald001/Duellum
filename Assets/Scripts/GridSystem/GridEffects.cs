using System.Collections.Generic;
using UnityEngine;

public class GridEffects : MonoBehaviour {
    [Header("Raise")]
    [SerializeField] BaseSelector RaiseSelector;
    [SerializeField] private int tileAmount;
    [SerializeField] private int directionIndex;
    [SerializeField] private float height;

    [Header("Ripple")]
    [SerializeField] BaseSelector RippleSelector;
    [SerializeField] private int rippleRange;
    [SerializeField] private float rippleStrength;

    private void Update() {
        if (MouseToWorldView.HoverTileGridPos == GridStaticFunctions.CONST_EMPTY)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Ripple(MouseToWorldView.HoverTileGridPos, rippleRange, rippleStrength);
        if (Input.GetKeyDown(KeyCode.Mouse1))
            Raise(MouseToWorldView.HoverTileGridPos, Input.GetKey(KeyCode.LeftShift), tileAmount, height);
    }

    private void Ripple(Vector2Int gridPos, int rippleRange, float rippleStrength) {
        List<Vector2Int> positions = RippleSelector.GetAvailableTiles(gridPos, rippleRange, 6);

        for (int i = 0; i < positions.Count; i++) {
            Hex currentHex = GridStaticFunctions.Grid[positions[i]];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                    new WaitAction(Mathf.Pow(i, i / 70f) - Mathf.Pow(1, 1 / 70f)),
                    new MoveObjectAction(currentHex.gameObject, 50 / Mathf.Pow(i, i / 70f), currentHex.StandardPosition - new Vector3(0, rippleRange / rippleStrength / Mathf.Pow(i, i / 10f), 0)),
                    new MoveObjectAction(currentHex.gameObject, 2 / Mathf.Pow(i, i / 10f), currentHex.StandardPosition),
                };
            currentHex.SetActionQueue(queue);
        }

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .4f);
    }

    private void Raise(Vector2Int gridPos, bool invert, int tileAmount, float height) {
        List<Vector2Int> positions = RaiseSelector.GetAvailableTiles(gridPos, tileAmount, 6);

        for (int i = 0; i < positions.Count; i++) {
            float newHeight = invert ? -height : height;
            Hex currentHex = GridStaticFunctions.Grid[positions[i]];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                    new MoveObjectAction(currentHex.gameObject, 50 / Mathf.Pow(i, i / 70f), currentHex.StandardPosition + new Vector3(0, newHeight, 0)),
                    new DoMethodAction(() => currentHex.StandardPosition = currentHex.transform.position),
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