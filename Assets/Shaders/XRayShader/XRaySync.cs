using UnityEngine;

public class XRaySync : MonoBehaviour {
    [SerializeField] private GameObject triggerObject;

    private Camera Camera;
    private Plane plane;

    private void Start() {
        Camera = Camera.main;

        plane = new(Vector3.up, Vector3.zero);
    }

    private void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter)) {
            Vector3 hitPoint = ray.GetPoint(enter);
            float dis = Vector3.Distance(Camera.transform.position, hitPoint);

            Vector3 newPos = (hitPoint + Camera.transform.position) / 2;
            triggerObject.transform.position = newPos;
            triggerObject.transform.localScale = new(1, 1, dis / 2);
            triggerObject.transform.LookAt(hitPoint);
        }
    }
}
