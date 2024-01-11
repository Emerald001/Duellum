using System.Collections;
using UnityEngine;

public class DungeonDoor : MonoBehaviour, IInteractable {
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [SerializeField] private float openeningSpeed;

    public bool IsOpened { get; private set; } = false;

    public void OnInteract() {
        StartCoroutine(OpenDoorSequence());
    }

    private IEnumerator OpenDoorSequence() {
        //EventManager<BattleEvents, bool>.Invoke(CameraEventType., false);

        Quaternion leftTargetRotation = leftDoor.rotation * Quaternion.Euler(0, 90, 0);
        Quaternion rightTargetRotation = leftDoor.rotation * Quaternion.Euler(0, -90, 0);

        while (leftDoor.rotation != leftTargetRotation) {
            leftDoor.rotation = Quaternion.Lerp(leftDoor.rotation, leftTargetRotation, openeningSpeed * Time.deltaTime);
            rightDoor.rotation = Quaternion.Lerp(rightDoor.rotation, rightTargetRotation, openeningSpeed * Time.deltaTime);

            yield return null;
        }

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .3f);

        yield return new WaitForSeconds(1f);

        IsOpened = true;
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, true);
    }
}
