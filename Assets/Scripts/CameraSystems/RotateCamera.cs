using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [SerializeField] private float rotSpeed;

    private ActionQueue queue = new();

    private readonly float[] yRots = {
        30,
        90,
        150,
        210,
        270,
        330
    };

    private bool canInvoke = true;
    private int index;

    private void Start() {
        queue = new(() => canInvoke = true);
        Rotate(0);
    }

    private void Update() {
        queue.OnUpdate();

        if (!canInvoke)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            Rotate(-1);
        if (Input.GetKeyDown(KeyCode.Q))
            Rotate(1);
    }

    private void Rotate(int dir) {
        canInvoke = false;

        index += dir;
        if (index > 5)
            index = 0;
        if (index < 0)
            index = 5;

        queue.Enqueue(new RotateAction(gameObject, new Vector3(0, yRots[index], 0), rotSpeed, .01f));
    }
}