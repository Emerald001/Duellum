using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {
    [SerializeField] private new Renderer renderer;

    [Header("Colors")]
    [SerializeField] private Material standardColor;
    [SerializeField] private Material HoverColor;
    [SerializeField] private Material movementColor;
    [SerializeField] private Material attackColor;

    public Vector2Int GridPos { get; set; }
    public Vector3 StandardWorldPosition { get; set; }

    private HighlightType currentType = HighlightType.None;
    private readonly ActionQueue queue = new();

    private void Update() {
        queue.OnUpdate();
    }

    public void SetHighlight(HighlightType type) {
        renderer.material = type switch {
            HighlightType.None => standardColor,
            HighlightType.MovementHighlight => movementColor,
            HighlightType.AttackHighlight => attackColor,
            _ => throw new System.NotImplementedException(),
        };

        currentType = type;
    }

    public void SetHover(bool hover) {
        if (hover)
            renderer.material = HoverColor;
        else
            SetHighlight(currentType);
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue() => queue.Clear();
}

public enum HighlightType {
    None,
    MovementHighlight,
    AttackHighlight,
}