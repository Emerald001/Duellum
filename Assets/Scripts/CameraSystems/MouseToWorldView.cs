using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseToWorldView : MonoBehaviour {
    public static Vector2Int HoverTileGridPos { get; set; } = GridStaticFunctions.CONST_EMPTY;
    public static Vector3 HoverPointPos { get; set; }

    [SerializeField] private Material hovercolor;
    [SerializeField] private Selector standardSelector;

    private Selector displaySelector;

    private readonly List<Hex> lastTiles = new();

    private void Start() {
        displaySelector = standardSelector;
    }

    private void OnEnable() {
        EventManager<CameraEventType, Selector>.Subscribe(CameraEventType.CHANGE_CAM_SELECTOR, GiveSelector);
    }

    private void OnDisable() {
        EventManager<CameraEventType, Selector>.Unsubscribe(CameraEventType.CHANGE_CAM_SELECTOR, GiveSelector);
    }

    void Update() {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 10000))
            return;

        UpdateTileColors(hit);

        HoverPointPos = hit.point;
    }

    private void UpdateTileColors(RaycastHit hit) {
        if (!hit.transform.CompareTag("WalkableTile")) {
            if (lastTiles.Count > 0) {
                for (int i = 0; i < lastTiles.Count; i++) {
                    if (lastTiles[i] != null)
                        lastTiles[i].SetHover(false);
                }

                HoverTileGridPos = GridStaticFunctions.GetGridPosFromHexGameObject(null);
            }
            return;
        }

        GameObject hitTile = hit.transform.parent.gameObject;
        List<Vector2Int> newTiles = GridStaticSelectors.GetPositions(displaySelector, GridStaticFunctions.GetGridPosFromHexGameObject(hitTile));

        if (newTiles.Contains(GridStaticFunctions.CONST_EMPTY))
            newTiles.Remove(GridStaticFunctions.CONST_EMPTY);

        foreach (var lastTile in lastTiles) {
            if (lastTile != null)
                lastTile.SetHover(newTiles.Contains(lastTile.GridPos));
        }

        lastTiles.Clear();
        lastTiles.AddRange(newTiles.Select(x => GridStaticFunctions.Grid[x]));
        HoverTileGridPos = GridStaticFunctions.GetGridPosFromHexGameObject(hitTile);
    }

    private void GiveSelector(Selector selector = null) {
        if (selector == null)
            displaySelector = standardSelector;
        else
            displaySelector = selector;
    }
}