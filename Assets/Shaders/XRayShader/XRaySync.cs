using System.Linq;
using UnityEngine;

public class XRaySync : MonoBehaviour {
    [SerializeField] private Camera Camera;
    [SerializeField] private GameObject cone;

    private void Update() {
        var dis = Vector3.Distance(Camera.transform.position, transform.position);

        cone.transform.position = Camera.transform.position;
        cone.transform.localScale = new(1, 1, dis / 2);
        cone.transform.LookAt(transform.position);
    }
}
