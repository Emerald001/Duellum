using UnityEngine;

public class XRaySync : MonoBehaviour {
    [SerializeField] private GameObject triggerObject;

    private Camera Camera;
    private Plane plane;

    private void Start() {
        Camera = Camera.main;

        plane = new(Vector3.up, new Vector3(0, -10, 0));
    }

    private void Update() {
        Ray ray = Camera.ScreenPointToRay(Input.mousePosition);
        var startPoint = Camera.ScreenToWorldPoint(Input.mousePosition);

        if (plane.Raycast(ray, out float enter)) {
            Vector3 hitPoint = ray.GetPoint(enter);

            float dis = Vector3.Distance(startPoint, hitPoint);

            Vector3 newPos = (hitPoint + startPoint) / 2;
            triggerObject.transform.position = newPos;
            triggerObject.transform.localScale = new(1, 1, dis / 2);
            triggerObject.transform.LookAt(hitPoint);
        }
    }
}
