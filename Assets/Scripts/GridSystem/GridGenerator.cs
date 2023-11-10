using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Hex HexPrefab;
    [SerializeField] private Hex CubePrefab;
    [SerializeField] private Hex ExtraHexPrefab;

    [Header("Grid Settings")]
    [SerializeField] private bool isHexagons;
    [SerializeField] private int rings;
    [SerializeField] private int extraRings;
    [SerializeField] private float initialSpawnDelay;
    [SerializeField] private float initialSpawnSpeed;
    [Header("Heightmap Settings")]
    [SerializeField] private HeightMapGenerator generator;
    [Range(1, 20)]
    [SerializeField] private float scaler;

    private GameObject Parent;

    private Vector3 startpos;

    private readonly float hexWidth = 1.732f;
    private readonly float hexHeight = 2f;

    private readonly float squareWidth = 1;
    private readonly float squareHeight = 1;

    private Texture2D heightMap;

    private readonly List<Hex> Hexes = new();

    public void SpawnGrid() {
        heightMap = generator.GenerateHeightMap(rings + extraRings);

        GridStaticFunctions.HexHeight = hexHeight;
        GridStaticFunctions.HexWidth = hexWidth;
        GridStaticFunctions.StartPos = startpos;

        GridStaticFunctions.SquareHeight = squareHeight;
        GridStaticFunctions.SquareWidth = squareWidth;

        Parent = new() {
            name = "Grid"
        };

        GenerateGrid();
    }

    private void GenerateGrid() {

        if (isHexagons) {
            GridStaticFunctions.RippleThroughGridPositions(new Vector2Int(0, 0), rings + extraRings, (currentPos, i) => {
                Hex hex = Instantiate(i > rings ? ExtraHexPrefab : HexPrefab);
                hex.GridPos = currentPos;
                hex.transform.position = GridStaticFunctions.CalcHexWorldPos(currentPos);
                hex.transform.parent = Parent.transform;
                hex.name = $"Hexagon {currentPos.x}|{currentPos.y}";
                Hexes.Add(hex);
            }, false);
        }
        else {
            GridStaticFunctions.RippleThroughSquareGridPositions(new Vector2Int(0, 0), rings + extraRings, (currentPos, i) => {
                Hex hex = Instantiate(i > rings ? ExtraHexPrefab : CubePrefab);
                hex.GridPos = currentPos;
                hex.transform.position = GridStaticFunctions.CalcSquareWorldPos(currentPos);
                hex.transform.parent = Parent.transform;
                hex.name = $"Square {currentPos.x}|{currentPos.y}";
                Hexes.Add(hex);
            }, false);
        }
        Hexes.ForEach(x => GridStaticFunctions.Grid.Add(x.GridPos, x));

        int width = (rings + extraRings);
        Dictionary<Vector2Int, float> positions = new();
        for (int x = 0; x < heightMap.width; x++) {
            for (int y = 0; y < heightMap.height; y++) {
                Color pixel = heightMap.GetPixel(x, y);
                positions.Add(new Vector2Int(x - width, y - width), pixel.r);
            }
        }

        foreach (Hex hex in Hexes) {
            if (positions.TryGetValue(hex.GridPos, out float value)) {
                hex.SetHighlight(HighlightType.None);
                hex.SetActionQueue(new List<Action>() {
                    new WaitAction(initialSpawnDelay),
                    new MoveObjectAction(hex.gameObject, initialSpawnSpeed * value, hex.transform.position + new Vector3(0, (value * scaler), 0)),
                    new DoMethodAction(() => hex.StandardWorldPosition = hex.transform.position)
                });
            }
        }

        Camera.main.transform.position = new Vector3(0, rings * 2, -(rings * 2 + 2));
        Camera.main.transform.parent.position = new Vector3(0, GridStaticFunctions.Grid[new Vector2Int(0, 0)].transform.position.y, 0);
    }
}
