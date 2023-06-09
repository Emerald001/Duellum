using System.Collections.Generic;
using UnityEngine;

public class GridEffects : MonoBehaviour {
    [Header("Raise")]
    [SerializeField] private int tileAmount;
    [SerializeField] private float height;

    [Header("Ripple")]
    [SerializeField] private int rippleRange;
    [SerializeField] private float rippleStrength;

    private void OnEnable() {

    }

    private void Update() {
        if (MouseToWorldView.HoverTileGridPos == GridStaticFunctions.CONST_EMPTY)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Ripple(MouseToWorldView.HoverTileGridPos, rippleRange, rippleStrength);
        if (Input.GetKeyDown(KeyCode.Mouse1))
            Raise(MouseToWorldView.HoverTileGridPos, Input.GetKey(KeyCode.LeftShift), tileAmount, height);
    }

    private void Ripple(Vector2Int gridPos, int rippleRange, float rippleStrength) {
        GridStaticFunctions.RippleThroughGridPositions(gridPos, rippleRange, (currentPos, i) =>
        {
            Hex currentHex = GridStaticFunctions.Grid[currentPos];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                    new WaitAction(Mathf.Pow(i, i / 70f) - Mathf.Pow(1, 1 / 70f)),
                    new MoveObjectAction(currentHex.gameObject, 50 / Mathf.Pow(i, i / 70f), currentHex.StandardPosition - new Vector3(0, rippleRange / rippleStrength / Mathf.Pow(i, i / 10f), 0)),
                    new MoveObjectAction(currentHex.gameObject, 2 / Mathf.Pow(i, i / 10f), currentHex.StandardPosition),
                };
            currentHex.SetActionQueue(queue);
        });

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .4f);
    }

    private void Raise(Vector2Int gridPos, bool invert, int tileAmount, float height) {
        GridStaticFunctions.RippleThroughGridPositions(gridPos, tileAmount, (currentPos, i) =>
        {
            float newHeight = invert ? -height : height;
            Hex currentHex = GridStaticFunctions.Grid[currentPos];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                    new WaitAction(Mathf.Pow(i, i / 70f) - Mathf.Pow(1, 1 / 70f)),
                    new MoveObjectAction(currentHex.gameObject, 50 / Mathf.Pow(i, i / 70f), currentHex.StandardPosition + new Vector3(0, tileAmount / newHeight / Mathf.Pow(i, i / 10f), 0)),
                    new DoMethodAction(() => currentHex.StandardPosition = currentHex.transform.position),
                };
            currentHex.SetActionQueue(queue);
        });

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .4f);
    }
}

public enum GridEffectsEvents {
    DO_RIPPLE,
    RAISE_TILE,
    LOWER_TILE,
}