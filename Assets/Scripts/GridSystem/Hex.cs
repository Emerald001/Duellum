using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour {
    public Material BaseColor;

    public Material GivenColor { get; set; }

    public Vector2Int GridPos { get; set; }
    public Vector3 StandardPosition { get; set; }

    private readonly ActionQueue Queue = new();

    private void Update() {
        Queue.OnUpdate();
    }

    public void SetColor(Material color = null) {
        if (color == null)
            GetComponentInChildren<Renderer>().material = BaseColor;
        else
            GetComponentInChildren<Renderer>().material = GivenColor = color;
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            Queue.Enqueue(item);
    }

    public void ClearQueue() => Queue.Clear();
}