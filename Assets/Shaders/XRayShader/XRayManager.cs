using System.Collections.Generic;
using UnityEngine;

public class XRayManager : MonoBehaviour {
    public static int PosID = Shader.PropertyToID("_PlayerPos");
    public static int SizeID = Shader.PropertyToID("_CircleSize");

    [SerializeField] private float holeSize;
    [SerializeField] private CapsuleCollider trigger;

    private readonly List<Renderer> currentHits = new();
    private List<Renderer> previousHits = new();

    private Plane plane;
    private bool canInvoke = false;

    private void Awake() {
        plane = new Plane(Vector3.up, Vector3.zero);
        trigger.radius = holeSize * 2;
    }

    private void OnEnable() {
        EventManager<CameraEventType, float>.Subscribe(CameraEventType.ChangeXraySize, SetSizes);
    }
    private void OnDisable() {
        EventManager<CameraEventType, float>.Unsubscribe(CameraEventType.ChangeXraySize, SetSizes);
    }

    private void SetSizes(float size) {
        holeSize = size;
        trigger.radius = holeSize* 2;
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.layer != 6)
            return;

        Renderer rend = other.GetComponent<Renderer>();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
            rend.enabled = false;

        currentHits.Add(rend);
        canInvoke = true;
    }

    private void LateUpdate() {
        if (!canInvoke)
            return;

        for (int i = 0; i < previousHits.Count; i++) {
            if (!currentHits.Contains(previousHits[i]))
                previousHits[i].enabled = true;
        }

        previousHits = new(currentHits);
        currentHits.Clear();
        canInvoke = false;
    }
}
