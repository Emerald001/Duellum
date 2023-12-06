using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private float moveSpeed;

    private void OnEnable() {
        EventManager<CameraEventType, Transform>.Subscribe(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, ChangeCameraFollowObject);
    }
    private void OnDisable() {
        EventManager<CameraEventType, Transform>.Unsubscribe(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, ChangeCameraFollowObject);
    }

    private void ChangeCameraFollowObject(Transform transform) {
        this.transform.position = transform.position;
        this.transform.SetParent(transform);
    }
}
