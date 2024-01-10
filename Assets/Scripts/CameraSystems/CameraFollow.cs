using UnityEngine;

public class CameraFollow : MonoBehaviour {
    private Transform followObject;
    private float moveSpeed;

    private void OnEnable() {
        EventManager<CameraEventType, Transform>.Subscribe(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, ChangeCameraFollowObject);
        EventManager<CameraEventType, float>.Subscribe(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, SetFollowSpeed);
    }
    private void OnDisable() {
        EventManager<CameraEventType, Transform>.Unsubscribe(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, ChangeCameraFollowObject);
        EventManager<CameraEventType, float>.Unsubscribe(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, SetFollowSpeed);
    }

    private void ChangeCameraFollowObject(Transform transform) {
        followObject = transform;
    }

    private void SetFollowSpeed(float speed) {
        moveSpeed = speed;
    }

    private void QuickZoom(float zoomAmount, Transform target, float duration) {

    }

    private void LateUpdate() {
        if (followObject == null)
            return;

        transform.position = Vector3.Lerp(transform.position, followObject.position, Time.deltaTime * moveSpeed);
    }
}
