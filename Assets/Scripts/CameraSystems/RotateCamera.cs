using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    [SerializeField] private float rotSpeed;

    private ActionQueue queue = new();

    private readonly float[] yRots = {
        0, 
        60,
        120,
        180,
        240,
        300
    };

    private bool canInvoke = true;
    private int index;

    private void Start() {
        queue = new(() => canInvoke = true);
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

public class RotateAction : Action {
    public RotateAction(GameObject go, Vector3 rot, float speed, float precision) {
        this.go = go;
        rotateTo = rot;
        this.speed = speed;
        this.precision = precision;
    }

    private readonly GameObject go;
    private readonly Vector3 rotateTo;
    private readonly float speed;
    private readonly float precision;

    public override void OnEnter() { }
    public override void OnExit() { }

    public override void OnUpdate() {
        if (Vector3.Distance(go.transform.eulerAngles, rotateTo) > precision)
            go.transform.rotation = Quaternion.Lerp(go.transform.rotation, Quaternion.Euler(rotateTo), Time.deltaTime * speed);
        else
            IsDone = true;
    }
}