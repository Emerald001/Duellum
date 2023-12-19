using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRaySync : MonoBehaviour
{
    public static int PosID = Shader.PropertyToID("_PlayerPos");
    public static int SizeID = Shader.PropertyToID("_CircleSize");

    public Material WallMaterial;
    public Camera Camera;
    public LayerMask Mask;
    void Update()
    {
        var dir = Camera.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);
        var dis = Vector3.Distance(Camera.transform.position, transform.position);

        if (Physics.Raycast(ray, dis, Mask))
            WallMaterial.SetFloat(SizeID, 1);
        else
            WallMaterial.SetFloat(SizeID, 0);

        var view = Camera.WorldToViewportPoint(transform.position);
        WallMaterial.SetVector(PosID, view);
    }
}
