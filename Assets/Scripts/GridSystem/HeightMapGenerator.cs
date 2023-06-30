using UnityEngine;

[System.Serializable]
public class HeightMapGenerator {
    public float scale = 10f; // Scale of the noise
    public int octaves = 6; // Number of noise layers
    public float persistence = 0.5f; // Controls the amplitude of each octave
    public float lacunarity = 2f; // Controls the frequency of each octave
    public float roughness = .1f;

    public Texture2D GenerateHeightMap(int rings) {
        int width = rings * 2;
        float offset = Random.Range(0, (float)width);
        Texture2D heightMap = new(width, width);

        for (int y = 0; y < width; y++) {
            for (int x = 0; x < width; x++) {
                float normalizedHeight = CalculateHeight(x, y, offset);
                int heightValue = Mathf.RoundToInt(normalizedHeight * 10);

                // Assign color based on the height value
                Color color = Color.black;
                switch (heightValue) {
                    case 0: color = Color.black; break; // Water
                    case 1: color = new Color32(50, 50, 50, 255); break; // Base battlefield
                    case 2: color = new Color32(50, 50, 50, 255); break; // Base battlefield
                    case 3: color = new Color32(50, 50, 50, 255); break; // Base battlefield
                    case 4: color = new Color32(50, 50, 50, 255); break; // Base battlefield
                    case 5: color = new Color32(75, 75, 75, 255); break; // Raised battlefield
                    case 6: color = new Color32(75, 75, 75, 255); break; // Raised battlefield
                    case 7: color = new Color32(75, 75, 75, 255); break; // Raised battlefield
                    case 8: color = new Color32(125, 125, 125, 255); break; // Mountains
                    case 9: color = new Color32(225, 225, 225, 255); break; // Mountains
                    case 10: color = Color.white; break; // Snowy Peaks
                }

                float noise = Mathf.PerlinNoise((x + offset) / scale, (y + offset) / scale) * roughness;
                color.r += noise;
                heightMap.SetPixel(x, y, color);
            }
        }

        heightMap.Apply();
        return heightMap;
    }

    private float CalculateHeight(int x, int y, float offset) {
        float amplitude = 1f;
        float frequency = 1f;
        float heightValue = 0f;

        for (int i = 0; i < octaves; i++) {
            float perlinX = offset + x / scale * frequency;
            float perlinY = offset + y / scale * frequency;
            float perlinValue = Mathf.PerlinNoise(perlinX, perlinY) * 2f - 1f;
            heightValue += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        heightValue = (heightValue + 1f) / 2f;
        return heightValue;
    }
}
