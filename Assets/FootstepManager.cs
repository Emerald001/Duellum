using UnityEngine;

public class FootstepManager : MonoBehaviour {
    public PlayerController controller;
    public Transform leftFootTransform;
    public Transform rightFootTransform;
    public string groundTag = "WalkableTile";

    void Update() {

    }

    public void CheckFootstep(Transform footTransform) {
        RaycastHit hit;
        if (Physics.Raycast(footTransform.position, Vector3.down, out hit, 0.1f) && hit.collider.CompareTag(groundTag)) {
            //footstepSound.pitch = Mathf.Clamp(10, 0.5f, 2.0f);
            EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "HeavyFootsteps");
        }
    }
}
