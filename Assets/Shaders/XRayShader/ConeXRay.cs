using System.Collections.Generic;
using UnityEngine;

public class ConeXRay : MonoBehaviour {
    public static int PosID = Shader.PropertyToID("_PlayerPos");
    public static int SizeID = Shader.PropertyToID("_CircleSize");

    [SerializeField] private float holeSize;
    [SerializeField] private Material WallMaterial;
    [SerializeField] private LayerMask Mask;
    [SerializeField] private Transform player;

    private readonly List<Collider> currentHits = new();
    private List<Collider> previousHits = new();

    private Camera Camera;
    private bool canInvoke = false;

    private void Awake() {
        Camera = Camera.main;
    }

    private void OnTriggerStay(Collider other) {
        var dir = player.position - Camera.transform.position;
        var ray = new Ray(Camera.transform.position, dir.normalized);
        var dis = Vector3.Distance(Camera.transform.position, player.position);

        if (Physics.Raycast(ray, out var hit, dis, Mask)) {
            var mat = other.GetComponent<Renderer>().material;
            mat.SetFloat(SizeID, holeSize);
            mat.SetVector(PosID, Camera.WorldToViewportPoint(hit.point));
        }

        currentHits.Add(other);
        canInvoke = true;
    }

    private void LateUpdate() {
        if (!canInvoke)
            return;

        foreach (var item in previousHits) {
            if (!currentHits.Contains(item)) {
                var mat = item.GetComponent<Renderer>().material;
                mat.SetFloat(SizeID, 0);
            }
        }
        
        previousHits = new(currentHits);
        currentHits.Clear();
        canInvoke = false;
    }
}
