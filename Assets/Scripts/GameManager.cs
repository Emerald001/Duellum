using UnityEngine;

public class GameManager : MonoBehaviour {
    private void Start() {
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        Tooltip.HideTooltip_Static();
    }
}
