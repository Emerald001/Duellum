using UnityEditor;
using UnityEngine;

public class RoomGeneratorEditor : EditorWindow {
    private Vector2Int size = new Vector2Int(5, 5);
    private int tilesPerRoom = 25;
    private TileState[,] grid;
    private ButtonState[,] connectionGrid;

    private Vector2Int lastSize = new();

    private enum TileState {
        Empty,
        Filled
    }

    private enum ButtonState {
        None,
        ConnectionTop,
        ConnectionRight,
        ConnectionLeft,
        ConnectionDown
    }

    [MenuItem("Custom/Room Generator")]
    public static void ShowWindow() {
        GetWindow(typeof(RoomGeneratorEditor));
    }

    private void OnGUI() {
        GUILayout.Label("Room Generator", EditorStyles.boldLabel);

        size = EditorGUILayout.Vector2IntField("Size", size);
        tilesPerRoom = EditorGUILayout.IntField("Tiles Per Room", tilesPerRoom);

        if (size != lastSize) {
            GenerateRoom();
            lastSize = size;
        }

        if (GUILayout.Button("Reset"))
            GenerateRoom();

        if (grid != null) {

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    EditorGUILayout.BeginHorizontal();

                    if (y % tilesPerRoom == 0) {
                        GUILayout.Button("", GUILayout.ExpandWidth(false), GUILayout.Height(20 * tilesPerRoom));
                    }

                    for (int i = 0; i < tilesPerRoom; i++) {

                        for (int j = 0; j < tilesPerRoom; j++) {
                            GUI.backgroundColor = grid[j, y] == TileState.Empty ? Color.white : Color.gray;

                            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                                ToggleTileState(x + j, y + i);

                            if (j % tilesPerRoom == tilesPerRoom - 1 && j < size.x * tilesPerRoom - 1)
                                EditorGUILayout.Space(.1f, false);
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginVertical();

                        if (y % tilesPerRoom == tilesPerRoom - 1 && y < size.y * tilesPerRoom - 1)
                            EditorGUILayout.Space();

                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }
    }

    private void GenerateRoom() {
        grid = new TileState[size.x * tilesPerRoom , size.y * tilesPerRoom];

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                grid[x, y] = TileState.Empty;
            }
        }
    }

    private void ToggleTileState(int x, int y) {
        grid[x, y] = grid[x, y] == TileState.Empty ? TileState.Filled : TileState.Empty;
    }
}