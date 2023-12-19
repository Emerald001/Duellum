using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XRaySync : MonoBehaviour {
    public static int PosID = Shader.PropertyToID("_PlayerPos");
    public static int SizeID = Shader.PropertyToID("_CircleSize");

    [SerializeField] private Material WallMaterial;
    [SerializeField] private Camera Camera;
    [SerializeField] private LayerMask Mask;

    private List<Collider> previousHits = new();

    private void Update() {
        var dir = transform.position - Camera.transform.position;
        var ray = new Ray(Camera.transform.position, dir.normalized);
        var dis = Vector3.Distance(Camera.transform.position, transform.position);

        RaycastHit[] tmp = Physics.RaycastAll(ray, dis, Mask);
        foreach (var item in tmp) {
            var mat = item.collider.GetComponent<Renderer>().material;
            mat.SetFloat(SizeID, 1);
            mat.SetVector(PosID, Camera.WorldToViewportPoint(item.point));
        }

        foreach (var item in previousHits) {
            if (!tmp.Any(hit => hit.collider == item)) {
                var mat = item.GetComponent<Renderer>().material;
                mat.SetFloat(SizeID, 0);
            }
        }
        previousHits = tmp.Select(x => x.collider).ToList();
    }
}
