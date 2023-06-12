using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {
    public Material BaseColor;

    public Material GivenColor { get; set; }

    public Vector2Int GridPos { get; set; }
    public Vector3 StandardPosition { get; set; }

    private readonly ActionQueue queue = new();

    private void Update() {
        queue.OnUpdate();
    }

    public void SetColor(Material color = null) {
        if (color == null)
            GetComponentInChildren<Renderer>().material = BaseColor;
        else
            GetComponentInChildren<Renderer>().material = GivenColor = color;
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue() => queue.Clear();
}