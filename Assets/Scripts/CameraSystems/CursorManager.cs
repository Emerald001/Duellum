using UnityEngine;

public class CursorManager : MonoBehaviour {
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D grabCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D movementCursor;
    [SerializeField] private Texture2D blockedCursor;

    private void OnEnable() {
        EventManager<UIEvents, CursorType>.Subscribe(UIEvents.UpdateCursor, SetCursor);
    }
    private void OnDisable() {
        EventManager<UIEvents, CursorType>.Unsubscribe(UIEvents.UpdateCursor, SetCursor);
    }

    private void SetCursor(CursorType type) {
        Cursor.SetCursor(GetTexture(type), Vector2.zero, CursorMode.Auto);
    }

    private Texture2D GetTexture(CursorType type) {
        return type switch {
            CursorType.Normal => normalCursor,
            CursorType.Hover => hoverCursor,
            CursorType.Grab => grabCursor,
            CursorType.Attack => attackCursor,
            CursorType.Move => movementCursor,
            CursorType.Blocked => blockedCursor,

            _ => throw new System.NotImplementedException(),
        };
    }
}

public enum CursorType {
    Normal,
    Hover,
    Grab,
    Attack,
    Move, 
    Blocked,
}