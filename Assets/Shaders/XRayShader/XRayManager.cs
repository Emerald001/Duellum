using System.Collections.Generic;
using UnityEngine;

public class XRayManager : MonoBehaviour {
    public static int PosID = Shader.PropertyToID("_PlayerPos");
    public static int SizeID = Shader.PropertyToID("_CircleSize");

    [SerializeField] private float holeSize;
    [SerializeField] private CapsuleCollider trigger;

    private readonly List<Collider> currentHits = new();
    private List<Collider> previousHits = new();

    private Camera Camera;
    private Plane plane;
    private bool canInvoke = false;

    private void Awake() {
        Camera = Camera.main;

        plane = new Plane(Vector3.up, Vector3.zero);
    }

    private void OnEnable() {
        EventManager<CameraEventType, float>.Subscribe(CameraEventType.ChangeXraySize, (x) => holeSize = x);
    }
    private void OnDisable() {
        EventManager<CameraEventType, float>.Unsubscribe(CameraEventType.ChangeXraySize, (x) => holeSize = x);
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.layer != 6)
            return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter)) {
            Vector3 hitPoint = ray.GetPoint(enter);

            Material mat = other.GetComponent<Renderer>().material;
            mat.SetFloat(SizeID, holeSize);
            mat.SetVector(PosID, Camera.WorldToViewportPoint(hitPoint));
        }

        currentHits.Add(other);
        canInvoke = true;
    }

    private void LateUpdate() {
        if (!canInvoke)
            return;

        foreach (Collider item in previousHits) {
            if (!currentHits.Contains(item)) {
                Material mat = item.GetComponent<Renderer>().material;
                mat.SetFloat(SizeID, 0);
            }
        }

        trigger.radius = holeSize;

        previousHits = new(currentHits);
        currentHits.Clear();
        canInvoke = false;
    }
}
