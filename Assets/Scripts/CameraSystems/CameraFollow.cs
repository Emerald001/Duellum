using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    private Transform followObject;
    private float moveSpeed;

    private void OnEnable() {
        EventManager<CameraEventType, EventMessage<Transform, float, float>>.Subscribe(CameraEventType.QuickZoom, (x) => StartCoroutine(QuickZoom(x)));
        EventManager<CameraEventType, Transform>.Subscribe(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, ChangeCameraFollowObject);
        EventManager<CameraEventType, float>.Subscribe(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, SetFollowSpeed);
    }
    private void OnDisable() {
        EventManager<CameraEventType, EventMessage<Transform, float, float>>.Unsubscribe(CameraEventType.QuickZoom, (x) => StartCoroutine(QuickZoom(x)));
        EventManager<CameraEventType, Transform>.Unsubscribe(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, ChangeCameraFollowObject);
        EventManager<CameraEventType, float>.Unsubscribe(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, SetFollowSpeed);
    }

    private void ChangeCameraFollowObject(Transform transform) {
        followObject = transform;
    }

    private void SetFollowSpeed(float speed) {
        moveSpeed = speed;
    }

    private IEnumerator QuickZoom(EventMessage<Transform, float, float> message) {
        Transform target = message.value1; 
        float zoomAmount = message.value2; 
        float duration = message.value3;

        float previousMoveSpeed = moveSpeed;
        moveSpeed = 10;

        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, false);
        EventManager<CameraEventType, EventMessage<float, float>>.Invoke(CameraEventType.ZOOM_CAM, new(zoomAmount, 10));

        var lastTarget = followObject;
        followObject = target;

        yield return new WaitForSeconds(duration);

        followObject = lastTarget;
        moveSpeed = previousMoveSpeed;

        EventManager<CameraEventType, EventMessage<float, float>>.Invoke(CameraEventType.ZOOM_CAM, new(7, 10));
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, true);
    }

    private void LateUpdate() {
        if (followObject == null)
            return;

        transform.position = Vector3.Lerp(transform.position, followObject.position, Time.deltaTime * moveSpeed);
    }
}
