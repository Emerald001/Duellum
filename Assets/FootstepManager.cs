using UnityEngine;

public class FootstepManager : MonoBehaviour {
    public PlayerController controller;
    public AudioSource footstepSound;
    public Transform leftFootTransform;
    public Transform rightFootTransform;
    public LayerMask groundLayer;
    public float checkInterval = 0.2f; // Adjust the interval as needed
    public string groundTag = "WalkableTile";
    float lastCheckTime;

    void Update() {
        
    }

    public void CheckFootstep(Transform footTransform) {
        RaycastHit hit;
        if (Physics.Raycast(footTransform.position, Vector3.down, out hit, 0.1f) && hit.collider.CompareTag(groundTag)) {
            Debug.Log("step");
            footstepSound.pitch = Mathf.Clamp(10, 0.5f, 2.0f);
            footstepSound.PlayOneShot(footstepSound.clip);
        }
    }
}
