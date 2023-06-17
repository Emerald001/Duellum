using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject plane;
    [SerializeField] private Hex HexPrefab;
    [SerializeField] private Hex ExtraHexPrefab;

    [SerializeField] private int rings;
    [SerializeField] private int extraRings;
    [Range(.01f, 5)]
    [SerializeField] private float roughness;
    [Range(1, 20)]
    [SerializeField] private float scaler;

    [SerializeField] private HeightMapGenerator generator;

    private void Awake() {
        new MakeGrid(HexPrefab, ExtraHexPrefab, generator.GenerateHeightMap(rings + extraRings), plane, rings, extraRings, roughness, scaler);
    }
}
