using UnityEngine;

public class XRaySync : MonoBehaviour {
    [SerializeField] private GameObject triggerObject;

    private Camera Camera;

    private void Awake() {
        Camera = Camera.main;
    }

    private void Update() {
        float dis = Vector3.Distance(Camera.transform.position, transform.position);

        Vector3 newPos = (transform.position + Camera.transform.position) / 2;
        triggerObject.transform.position = newPos;
        triggerObject.transform.localScale = new(1, 1, dis / 2);
        triggerObject.transform.LookAt(transform.position);
    }
}
