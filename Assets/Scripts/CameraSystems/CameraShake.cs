using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private ActionQueue queue;

    private void Start() {
        queue = new();
    }

    private void OnEnable() {
        EventManager<CameraEventType, float>.Subscribe(CameraEventType.DO_CAMERA_SHAKE, DoCameraShake);
    }

    private void OnDisable() {
        EventManager<CameraEventType, float>.Unsubscribe(CameraEventType.DO_CAMERA_SHAKE, DoCameraShake);
    }

    private void Update() {
        queue.OnUpdate();
    }

    private void DoCameraShake(float amount) {
        queue.Clear(true);
        queue.Enqueue(new ShakeObjectAction(transform, .1f, amount));
    }
}

public enum CameraEventType {
    DO_CAMERA_SHAKE,
}