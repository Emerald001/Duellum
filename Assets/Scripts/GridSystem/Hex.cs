using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {
    [SerializeField] private new Renderer renderer;

    public Renderer GivenRenderer => renderer;
    public Material GivenColor { get; set; }
    public Material BaseColor => baseMaterial;

    public Vector2Int GridPos { get; set; }
    public Vector3 StandardPosition { get; set; }

    private readonly ActionQueue queue = new();
    private Material baseMaterial;

    private void Update() {
        queue.OnUpdate();
    }

    public void SetBaseColor(Color color) {
        baseMaterial = new(renderer.material) {
            color = color
        };

        SetColor();
    }

    public void SetColor(Material color = null) {
        if (color == null)
            renderer.material = baseMaterial;
        else
            renderer.material = GivenColor = color;
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue() => queue.Clear();
}