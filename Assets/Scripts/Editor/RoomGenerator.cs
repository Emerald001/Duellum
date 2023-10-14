using UnityEditor;
using UnityEngine;

public class RoomGeneratorEditor : EditorWindow {
    private Vector2Int size = new(5, 5);
    private int tilesPerRoom = 25;
    private bool[,] grid;

    [MenuItem("Custom/Room Generator")]
    public static void ShowWindow() {
        GetWindow(typeof(RoomGeneratorEditor));
    }

    private void OnGUI() {
        GUILayout.Label("Room Generator", EditorStyles.boldLabel);

        size = EditorGUILayout.Vector2IntField("Size", size);
        tilesPerRoom = EditorGUILayout.IntField("Tiles Per Room", tilesPerRoom);

        if (GUILayout.Button("Generate Room")) {
            GenerateRoom();
        }

        if (grid != null) {
            GUILayout.Label("Click to toggle tiles:");

            for (int y = 0; y < size.y * tilesPerRoom; y++) {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < size.x * tilesPerRoom; x++) {
                    grid[x, y] = GUILayout.Toggle(grid[x, y], "", GUILayout.ExpandWidth(false));
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void GenerateRoom() {
        grid = new bool[size.x * tilesPerRoom, size.y * tilesPerRoom];

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                grid[x, y] = false;
            }
        }
    }
}