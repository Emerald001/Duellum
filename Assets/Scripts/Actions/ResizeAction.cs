using UnityEngine;

public class ResizeAction : Action {
    public ResizeAction(Transform objectToResize, float resizeSpeed, Vector3 newScale) {
        this.objectToResize = objectToResize;
        this.resizeSpeed = resizeSpeed;
        this.newScale = newScale;
    }

    private readonly Transform objectToResize;
    private readonly float resizeSpeed;
    private readonly Vector3 newScale;

    public override void OnEnter() { }

    public override void OnExit() { }

    public override void OnUpdate() {
        if (objectToResize.localScale != newScale)
            objectToResize.localScale = Vector3.MoveTowards(objectToResize.localScale, newScale, resizeSpeed * Time.deltaTime);
        else
            IsDone = true;
    }
}