using UnityEngine;

public class MouseToWorldView : MonoBehaviour
{
    public static Vector2Int HoverTileGridPos { get; set; }
    public static Vector3 HoverPointPos { get; set; }

    [SerializeField] private Material Hovercolor;
    private Hex lastTile;

    void Update() {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 10000))
            return;

        if (!hit.transform.CompareTag("WalkableTile")) {
            if (lastTile != null) {
                lastTile.SetColor();
                HoverTileGridPos = Vector2Int.zero;
            }

            HoverPointPos = hit.point;
            return;
        }

        GameObject hitTile = hit.transform.parent.gameObject;
        if (hitTile != lastTile) {
            lastTile?.SetColor();
            lastTile = hitTile.GetComponent<Hex>();
            lastTile.SetColor(Hovercolor);

            HoverTileGridPos = UnitStaticFunctions.GetGridPosFromWorldPos(hitTile);
        }

        HoverPointPos = hit.point;
    }
}