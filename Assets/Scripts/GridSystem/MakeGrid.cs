using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MakeGrid {
    private readonly Vector2Int[] evenNeighbours = {
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
        };

    private readonly Vector2Int[] unevenNeighbours = {
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
        };

    private readonly List<Hex> Hexes = new();
    private readonly Hex HexPrefab;
    private readonly Hex ExtraHexPrefab;
    private GameObject Parent;

    private Vector3 startpos;

    private readonly float roughness;
    private readonly float hexWidth = 1.732f;
    private readonly float hexHeight = 2f;
    private float scaler;

    private readonly int rings;
    private readonly int extraRings;

    public MakeGrid(Hex HexPrefab, Hex ExtraHexPrefab, int rings, int extraRings, float roughness, float scaler) {
        this.HexPrefab = HexPrefab;
        this.ExtraHexPrefab = ExtraHexPrefab;
        this.rings = rings;
        this.extraRings = extraRings;
        this.roughness = roughness;
        this.scaler = scaler;

        UnitStaticFunctions.HexHeight = hexHeight;
        UnitStaticFunctions.HexWidth = hexWidth;
        UnitStaticFunctions.StartPos = startpos;

        Parent = new() {
            name = "Grid"
        };

        GenerateGrid();
    }

    private void GenerateGrid() {
        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();
        Dictionary<Vector2Int, float> positions = new();
        float offset = Random.Range(0, 1f);
        scaler /= roughness;

        for (float x = -(rings + extraRings); x < rings + extraRings; x++) {
            for (float y = -(rings + extraRings); y < rings + extraRings; y++) {
                float xCoord = offset + x / ((rings + extraRings) * 2) * roughness;
                float yCoord = offset + y / ((rings + extraRings) * 2) * roughness;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                positions.Add(new Vector2Int((int)x, (int)y), sample);
            }
        }

        openList.Add(new Vector2Int(0, 0));
        for (int i = 0; i < rings + extraRings; i++) {
            foreach (Vector2Int currentPos in openList) {
                Vector2Int[] listToUse;

                if (currentPos.y % 2 != 0)
                    listToUse = unevenNeighbours;
                else
                    listToUse = evenNeighbours;

                foreach (Vector2Int neighbour in listToUse.Select(dir => currentPos + dir)) {
                    if (openList.Contains(neighbour) || closedList.Contains(neighbour) || layerList.Contains(neighbour))
                        continue;

                    layerList.Add(neighbour);
                }

                Hex hex = Object.Instantiate(i > rings ? ExtraHexPrefab : HexPrefab);

                hex.GridPos = currentPos;
                hex.transform.position = UnitStaticFunctions.CalcWorldPos(currentPos);
                hex.transform.parent = Parent.transform;
                hex.name = $"Hexagon {currentPos.x}|{currentPos.y}";
                Hexes.Add(hex);

                closedList.Add(currentPos);
            }

            openList.Clear();
            for (int j = 0; j < layerList.Count; j++)
                openList.Add(layerList[j]);

            layerList.Clear();
        }

        Hexes.ForEach(x => UnitStaticFunctions.Grid.Add(x.GridPos, x));

        float lowestValue = positions.Values.Min() * scaler;
        foreach (Hex hex in Hexes) {
            if (positions.TryGetValue(hex.GridPos, out float value)) {
                hex.transform.position += new Vector3(0, (value * scaler) - lowestValue, 0);
                hex.StandardPosition = hex.transform.position;
            }
        }

        Camera.main.transform.position = new Vector3(0, rings * 2, -(rings * 2 + 2));
        Camera.main.transform.parent.position = new Vector3(0, UnitStaticFunctions.Grid[new Vector2Int(0, 0)].transform.position.y, 0);
    }
}