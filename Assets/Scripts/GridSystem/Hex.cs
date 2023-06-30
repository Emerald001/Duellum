using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {
    [SerializeField] private new Renderer renderer;

    public Material GivenColor { get; set; }

    public Vector2Int GridPos { get; set; }
    public Vector3 StandardPosition { get; set; }

    private readonly ActionQueue queue = new();
    private Material BaseMaterial;

    private void Update() {
        queue.OnUpdate();
    }

    public void SetBaseColor(Color color) {
        BaseMaterial = new(renderer.material) {
            color = color
        };

        SetColor();
    }

    public void SetColor(Material color = null) {
        if (color == null)
            renderer.material = BaseMaterial;
        else
            renderer.material = GivenColor = color;
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue() => queue.Clear();
}