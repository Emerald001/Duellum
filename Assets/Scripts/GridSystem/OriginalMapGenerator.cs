using System.Collections.Generic;
using UnityEngine;

public class OriginalMapGenerator : MonoBehaviour {
    [SerializeField] private Hex gridCubePrefab;
    [SerializeField] private Hex gridWaterCubePrefab;
    [SerializeField] private Hex gridCardCubePrefab;
    [SerializeField] private Hex gridCoverCubePrefab;

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float gap;

    [Range(0, 100)]
    [SerializeField] private int waterPercentage;
    [Range(0, 100)]
    [SerializeField] private int cardPercentage;
    [Range(0, 100)]
    [SerializeField] private int coverPercentage;

    private readonly Dictionary<Vector2Int, Hex> GridToSpawn = new();
    private readonly Dictionary<Vector2Int, Hex> Grid = new();

    private GameObject gridParent;

    public void SetUp() {
        GridStaticFunctions.GridWidth = width;
        GridStaticFunctions.GridHeight = height;
        GridStaticFunctions.GridGap = gap;

        gridParent = new() {
            name = "Grid"
        };

        AllocateGrid();
        SpawnGrid();
    }

    private void SpawnGrid() {
        foreach (var item in GridToSpawn) {
            Hex tmp = Instantiate(item.Value);

            tmp.SetHighlight(HighlightType.None);
            tmp.GridPos = item.Key;
            tmp.StandardWorldPosition = GridStaticFunctions.CalcSquareWorldPos(item.Key);
            tmp.transform.position = GridStaticFunctions.CalcSquareWorldPos(item.Key);
            tmp.transform.SetParent(gridParent.transform);

            Grid.Add(item.Key, tmp);
        }

        GridStaticFunctions.Grid = Grid;
    }

    private void AllocateGrid() {
        List<Vector2Int> takenPositions = new();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector2Int gridPos = new(x, y);
                GridToSpawn.Add(gridPos, gridCubePrefab);
            }
        }

        GridStaticFunctions.RippleThroughSquareGridPositions(
            new Vector2Int(0, height / 2),
            3,
            (arg1, arg2) => {
                bool outOfBounds = arg1.x < 0 || arg1.x > width - 1 || arg1.y < 0 || arg1.y > height - 1; 
                if (!outOfBounds)
                    takenPositions.Add(arg1);
            },
            false);
        GridStaticFunctions.RippleThroughSquareGridPositions(
            new Vector2Int(width - 1, height / 2),
            3,
            (arg1, arg2) => {
                bool outOfBounds = arg1.x < 0 || arg1.x > width - 1 || arg1.y < 0 || arg1.y > height - 1;
                if (!outOfBounds)
                    takenPositions.Add(arg1);
            },
            false);

        GridStaticFunctions.PlayerSpawnPos.Add(new Vector2Int(0, height / 2));
        GridStaticFunctions.PlayerSpawnPos.Add(new Vector2Int(0, height / 2 - 1));
        GridStaticFunctions.PlayerSpawnPos.Add(new Vector2Int(0, height / 2 + 1));

        GridStaticFunctions.EnemySpawnPos.Add(new Vector2Int(width - 1, height / 2));
        GridStaticFunctions.EnemySpawnPos.Add(new Vector2Int(width - 1, height / 2 - 1));
        GridStaticFunctions.EnemySpawnPos.Add(new Vector2Int(width - 1, height / 2 + 1));

        int totalTiles = height * width;
        int waterTileAmount = Mathf.RoundToInt(totalTiles * ((float)waterPercentage / 100));
        int cardTileAmount = Mathf.RoundToInt(totalTiles * ((float)cardPercentage / 100));
        int coverTileAmount = Mathf.RoundToInt(totalTiles * ((float)coverPercentage / 100));

        AllocateRandomPositions(waterTileAmount, takenPositions, gridWaterCubePrefab);
        AllocateRandomPositions(cardTileAmount, takenPositions, gridCardCubePrefab);
        AllocateRandomPositions(coverTileAmount, takenPositions, gridCoverCubePrefab);
    }

    private void AllocateRandomPositions(int amount, List<Vector2Int> takenPositions, Hex prefab) {
        int counter = amount;
        while (counter > 0) {
            Vector2Int randomTile = new(Random.Range(0, width), Random.Range(0, height));

            if (!takenPositions.Contains(randomTile)) {
                takenPositions.Add(randomTile);
                GridToSpawn[randomTile] = prefab;
                counter--;
            }
        }
    }
}
