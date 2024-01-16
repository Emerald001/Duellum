using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private BattleData TMPData;

    private void Start() {
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        Tooltip.HideTooltip_Static();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.H)) {
            Transform target = GridStaticFunctions.Grid[Vector2Int.zero].transform;
            EventManager<CameraEventType, EventMessage<Transform, float, float>>.Invoke(CameraEventType.QuickZoom, new(target, 4, 3));
        }
    }
}
