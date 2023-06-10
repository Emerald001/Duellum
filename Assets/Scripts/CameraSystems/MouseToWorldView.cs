using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseToWorldView : MonoBehaviour
{
    public static Vector2Int HoverTileGridPos { get; set; } = GridStaticFunctions.CONST_EMPTY;
    public static Vector3 HoverPointPos { get; set; }

    [SerializeField] private Material Hovercolor;
    [SerializeField] private BaseSelector selector;
    private readonly List<Hex> lastTiles = new();

    void Update() {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 10000))
            return;

        UpdateTileColors(hit);

        HoverPointPos = hit.point;
    }

    private void UpdateTileColors(RaycastHit hit) {
        if (!hit.transform.CompareTag("WalkableTile")) {
            if (lastTiles.Count > 0) {
                lastTiles.ForEach(x => x.SetColor());
                HoverTileGridPos = GridStaticFunctions.GetGridPosFromHexGameObject(null);
            }
            return;
        }

        GameObject hitTile = hit.transform.parent.gameObject;
        List<Vector2Int> newTiles = selector.GetAvailableTiles(GridStaticFunctions.GetGridPosFromHexGameObject(hitTile), 5, 6);

        foreach (var lastTile in lastTiles) {
            if (newTiles.Contains(lastTile.GridPos))
                lastTile.SetColor(Hovercolor);
            else
                lastTile.SetColor();
        }

        lastTiles.Clear();
        lastTiles.AddRange(newTiles.Select(x => GridStaticFunctions.Grid[x]));
        HoverTileGridPos = GridStaticFunctions.GetGridPosFromHexGameObject(hitTile);
    }
}