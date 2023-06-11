using UnityEngine;

public class RotateAction : Action {
    public RotateAction(GameObject go, Vector3 rotateTo, float speed, float precision) {
        this.go = go;
        this.rotateTo = rotateTo;
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