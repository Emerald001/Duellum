using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseToWorldView : MonoBehaviour {
    public static Vector2Int HoverTileGridPos { get; set; } = GridStaticFunctions.CONST_EMPTY;

    [SerializeField] private Material hovercolor;
    [SerializeField] private Selector standardSelector;
    [SerializeField] private LayerMask hitRaycast;

    private List<Tile> lastTiles = new();
    private Selector displaySelector;
    private Camera mainCam;

    private void Awake() {
        displaySelector = standardSelector;
        mainCam = Camera.main;
    }

    private void OnEnable() {
        EventManager<CameraEventType, Selector>.Subscribe(CameraEventType.CHANGE_CAM_SELECTOR, GiveSelector);
    }
    private void OnDisable() {
        EventManager<CameraEventType, Selector>.Unsubscribe(CameraEventType.CHANGE_CAM_SELECTOR, GiveSelector);
    }

    private void Update() {
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 10000, hitRaycast))
            UpdateTileColors(hit);
        else
            ResetTiles();
    }

    private void UpdateTileColors(RaycastHit hit) {
        if (!hit.transform.CompareTag("WalkableTile")) {
            ResetTiles();
            return;
        }

        GameObject hitTile = hit.transform.parent.gameObject;
        Vector2Int hoverTileGridPos = GridStaticFunctions.GetGridPosFromTileGameObject(hitTile);
        List<Vector2Int> newTiles = GridStaticSelectors.GetPositions(displaySelector, hoverTileGridPos, 0)
            .Where(tile => tile != GridStaticFunctions.CONST_EMPTY)
            .ToList();

        foreach (var lastTile in lastTiles.Where(t => t != null))
            lastTile.SetHover(newTiles.Contains(lastTile.GridPos));

        lastTiles = newTiles.Select(x => GridStaticFunctions.Grid[x]).ToList();
        HoverTileGridPos = hoverTileGridPos;
    }

    private void ResetTiles() {
        if (lastTiles.Count > 0) {
            for (int i = 0; i < lastTiles.Count; i++) {
                if (lastTiles[i] != null)
                    lastTiles[i].SetHover(false);
            }

            HoverTileGridPos = GridStaticFunctions.GetGridPosFromTileGameObject(null);
            lastTiles.Clear();
        }
    }

    private void GiveSelector(Selector selector = null) {
        displaySelector = selector ?? standardSelector;
    }
}