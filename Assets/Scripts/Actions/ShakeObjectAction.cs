using UnityEngine;

public class ShakeObjectAction : Action {
    private readonly Transform transform;

    private readonly float shakeDuration = 2f;
    private readonly float shakeIntensity = 0.7f;

    private Vector3 orignalCameraPos;
    private float shakeTimer;

    public ShakeObjectAction(Transform transform, float duration, float intensity) {
        this.transform = transform;

        shakeDuration = duration;
        shakeIntensity = intensity;
    }

    public override void OnEnter() {
        orignalCameraPos = transform.localPosition;
        shakeTimer = shakeDuration;
    }

    public override void OnUpdate() {
        if (shakeTimer > 0) {
            transform.localPosition = orignalCameraPos + Random.insideUnitSphere * shakeIntensity;
            shakeTimer -= Time.deltaTime;
        }
        else {
            transform.localPosition = orignalCameraPos;
            IsDone = true;
        }
    }

    public override void OnExit() { }
}