using UnityEngine;

public class CameraZoom : MonoBehaviour {
    private Camera cam;

    private float zoomTarget;
    private float zoomSpeed;

    private void Awake() {
        cam = GetComponentInChildren<Camera>();
    }

    private void OnEnable() {
        EventManager<CameraEventType, EventMessage<float, float>>.Subscribe(CameraEventType.ZOOM_CAM, SetZoom);
    }
    private void OnDisable() {
        EventManager<CameraEventType, EventMessage<float, float>>.Unsubscribe(CameraEventType.ZOOM_CAM, SetZoom);
    }

    private void SetZoom(EventMessage<float, float> message) {
        zoomTarget = message.value1;
        zoomSpeed = message.value2;
    }

    private void Update() {
        if (cam.orthographicSize == zoomTarget)
            return;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomTarget, Time.deltaTime * zoomSpeed);
    }
}
