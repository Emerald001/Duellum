using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MakeGrid {
    private readonly List<Hex> Hexes = new();
    private readonly Hex HexPrefab;
    private readonly Hex ExtraHexPrefab;
    private readonly GameObject Parent;

    private GameObject Plane;

    private Vector3 startpos;

    private readonly float roughness;
    private readonly float hexWidth = 1.732f;
    private readonly float hexHeight = 2f;
    private float scaler;

    private readonly int rings;
    private readonly int extraRings;

    private Texture2D heightMap;

    public MakeGrid(Hex HexPrefab, Hex ExtraHexPrefab, Texture2D heightMap, GameObject plane, int rings, int extraRings, float roughness, float scaler) {
        this.HexPrefab = HexPrefab;
        this.ExtraHexPrefab = ExtraHexPrefab;
        this.rings = rings;
        this.extraRings = extraRings;
        this.roughness = roughness;
        this.scaler = scaler;
        this.Plane = plane;

        if (!heightMap)
            CreateRandomNoiseMap();
        else
            this.heightMap = heightMap;

        GridStaticFunctions.HexHeight = hexHeight;
        GridStaticFunctions.HexWidth = hexWidth;
        GridStaticFunctions.StartPos = startpos;

        Parent = new() {
            name = "Grid"
        };

        GenerateGrid();
    }

    private void CreateRandomNoiseMap() {
        int width = (rings + extraRings) * 2;
        float offset = Random.Range(0, 1f);
        Color[] pix = new Color[width * width];

        scaler /= roughness;
        for (float x = 0; x < width; x++) {
            for (float y = 0; y < width; y++) {
                float xCoord = offset + x / width * roughness;
                float yCoord = offset + y / width * roughness;
                
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                float editedSample = FloorTo(sample, 1, 10) * sample;

                pix[(int)y * width + (int)x] = new Color(editedSample, editedSample, editedSample);
            }
        }

        heightMap = new Texture2D(width, width);
        heightMap.SetPixels(pix);
        heightMap.Apply();

        Plane.GetComponent<Renderer>().material.mainTexture = heightMap;
    }

    private float FloorTo(float value, int place, uint @base) {
        if (place == 0)
            return (float)Mathf.Floor(value);
        else {
            float p = (float)Mathf.Pow(@base, place);
            return (float)Mathf.Floor(value * p) / p;
        }
    }

    private void GenerateGrid() {
        GridStaticFunctions.RippleThroughGridPositions(new Vector2Int(0, 0), rings + extraRings, (currentPos, i) => {
            Hex hex = Object.Instantiate(i > rings ? ExtraHexPrefab : HexPrefab);
            hex.GridPos = currentPos;
            hex.transform.position = GridStaticFunctions.CalcWorldPos(currentPos);
            hex.transform.parent = Parent.transform;
            hex.name = $"Hexagon {currentPos.x}|{currentPos.y}";
            Hexes.Add(hex);
        }, false);
        Hexes.ForEach(x => GridStaticFunctions.Grid.Add(x.GridPos, x));

        int width = (rings + extraRings);
        Dictionary<Vector2Int, float> positions = new();
        for (int x = 0; x < heightMap.width; x++) {
            for (int y = 0; y < heightMap.height; y++) {
                Color pixel = heightMap.GetPixel(x, y);
                positions.Add(new Vector2Int(x - width, y - width), pixel.r);
            }
        }

        float lowestValue = positions.Values.Min() * scaler;
        foreach (Hex hex in Hexes) {
            if (positions.TryGetValue(hex.GridPos, out float value)) {
                hex.transform.position += new Vector3(0, (value * scaler) - lowestValue, 0);
                hex.StandardPosition = hex.transform.position;
            }
        }

        Camera.main.transform.position = new Vector3(0, rings * 2, -(rings * 2 + 2));
        Camera.main.transform.parent.position = new Vector3(0, GridStaticFunctions.Grid[new Vector2Int(0, 0)].transform.position.y, 0);
    }
}