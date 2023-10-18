using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RoomGeneratorEditor : EditorWindow {
    private Vector2Int size = new(1, 1);
    private TileState[,] grid;
    private Vector4[,] connectionGrid;

    private Vector2Int lastSize = new();
    private int tilesPerRoom = 10;

    private enum TileState {
        Empty,
        Filled
    }

    private enum Directions {
        North,
        East,
        South,
        West
    }

    [MenuItem("Custom/Room Generator")]
    public static void ShowWindow() {
        GetWindow(typeof(RoomGeneratorEditor));
    }

    private void OnGUI() {
        GUILayout.Label("Room Generator", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("1x1 rooms are automatically added with the four rotations, others are not.", MessageType.Info);

        size = EditorGUILayout.Vector2IntField("Size", size);
        //tilesPerRoom = EditorGUILayout.IntField("Tiles Per Room", tilesPerRoom);

        if (size.x > 10)
            size = new(10, size.y);

        if (size.y > 10)
            size = new(size.x, 10);

        if (GUILayout.Button("Reset")) {
            AllocateRoomPositions();
            GenerateConnections();
        }

        if (GUILayout.Button("Generate Room")) {
            GenerateRoom();
        }

        if (size != lastSize) {
            AllocateRoomPositions();
            GenerateConnections();
            lastSize = size;
        }

        if (connectionGrid != null)
            DrawConnections();

        if (grid != null)
            DrawTiles();
    }

    private void DrawConnections() {
        DrawUILine(Color.gray);
        GUILayout.Label("Click on the buttons to identify their connections");

        EditorGUILayout.BeginVertical();

        // First Line
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(21f, false);
        for (int x = 0; x < size.x; x++) {
            GUI.backgroundColor = connectionGrid[x, 0].z == 1 ? Color.green : Color.white;

            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                ToggleConnectionState(x, 0, Directions.North);
        }
        EditorGUILayout.EndHorizontal();

        // Middle Line
        for (int y = 0; y < size.y; y++) {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < size.x + 2; x++) {
                if (x == 0) {
                    GUI.backgroundColor = connectionGrid[x, y].y == 1 ? Color.green : Color.white;

                    if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                        ToggleConnectionState(x, y, Directions.West);
                }
                else if (x == size.x + 1) {
                    GUI.backgroundColor = connectionGrid[x - 2, y].x == 1 ? Color.green : Color.white;

                    if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                        ToggleConnectionState(x - 2, y, Directions.East);
                }
                else {
                    GUI.backgroundColor = Color.gray;
                    GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // Last Line
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(21f, false);
        for (int x = 0; x < size.x; x++) {
            GUI.backgroundColor = connectionGrid[x, size.y - 1].w == 1 ? Color.green : Color.white;

            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                ToggleConnectionState(x, size.y - 1, Directions.South);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawTiles() {
        DrawUILine(Color.gray);

        GUILayout.Label("Click on the buttons to identify blockades");

        EditorGUILayout.BeginVertical();

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                GUI.backgroundColor = grid[x, y] == TileState.Empty ? Color.white : Color.gray;

                if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                    ToggleTileState(x, y);

                if (x % tilesPerRoom == tilesPerRoom - 1 && x < size.x * tilesPerRoom - 1)
                    EditorGUILayout.Space(.1f, false);
            }

            EditorGUILayout.EndHorizontal();

            if (y % tilesPerRoom == tilesPerRoom - 1 && y < size.y * tilesPerRoom - 1)
                EditorGUILayout.Space();
        }

        EditorGUILayout.EndVertical();
        DrawUILine(Color.gray);
    }

    private void DrawUILine(Color color, int thickness = 1, int padding = 10) {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }

    private void GenerateRoom() {
        EditorSceneManager.OpenScene("Assets/Scenes/RoomEditor.unity", OpenSceneMode.Single);

        GameObject room = GameObject.Find("New Room");
        if (room != null) {
            for (int i = room.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(room.transform.GetChild(0).gameObject);
        }

        GameObject hexGO = Resources.Load("GridBlocks/GridBlock") as GameObject;
        GameObject coverGO = Resources.Load("GridBlocks/GridCoverBlock") as GameObject;

        Hex hex = hexGO.GetComponent<Hex>();
        Hex coverHex = coverGO.GetComponent<Hex>();

        Transform parent = room != null ? room.transform : new GameObject("New Room").transform;

        GenerateGrid(parent, hex, coverHex);
    }

    private void GenerateGrid(Transform parent, Hex prefab, Hex coverPrefab) {
        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                Vector2Int gridPos = new(x, y);

                var tmp = Instantiate(grid[x, y] == TileState.Empty ? prefab : coverPrefab, parent);

                tmp.SetHighlight(HighlightType.None);
                tmp.GridPos = gridPos;
                tmp.StandardWorldPosition = CalcSquareWorldPos(gridPos);
                tmp.transform.position = CalcSquareWorldPos(gridPos);
                tmp.transform.SetParent(parent);
            }
        }
    }

    private Vector3 CalcSquareWorldPos(Vector2Int gridpos) {
        float x = gridpos.x - (((size.x * tilesPerRoom) - 1 + (0.02f * ((size.x * tilesPerRoom) - 1))) / 2) + 0.02f * gridpos.x;
        float z = gridpos.y - (((size.y * tilesPerRoom) - 1 + (0.02f * ((size.y * tilesPerRoom) - 1))) / 2) + 0.02f * gridpos.y;

        return new Vector3(x, 0, z);
    }

    private void AllocateRoomPositions() {
        grid = new TileState[size.x * tilesPerRoom , size.y * tilesPerRoom];

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++)
                grid[x, y] = TileState.Empty;
        }
    }

    private void GenerateConnections() {
        connectionGrid = new Vector4[size.x, size.y];
        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++)
                connectionGrid[x, y] = new Vector4();
        }
    }

    private void ToggleTileState(int x, int y) {
        grid[x, y] = grid[x, y] == TileState.Empty ? TileState.Filled : TileState.Empty;
    }

    private void ToggleConnectionState(int x, int y, Directions direction) {
        switch (direction) {
            case Directions.North:
                connectionGrid[x, y] = new(connectionGrid[x, y].x, connectionGrid[x, y].y, connectionGrid[x, y].z == 0 ? 1 : 0, connectionGrid[x, y].w);
                break;
            case Directions.East:
                connectionGrid[x, y] = new(connectionGrid[x, y].x == 0 ? 1 : 0, connectionGrid[x, y].y, connectionGrid[x, y].z, connectionGrid[x, y].w);
                break;
            case Directions.South:
                connectionGrid[x, y] = new(connectionGrid[x, y].x, connectionGrid[x, y].y, connectionGrid[x, y].z, connectionGrid[x, y].w == 0 ? 1 : 0);
                break;
            case Directions.West:
                connectionGrid[x, y] = new(connectionGrid[x, y].x, connectionGrid[x, y].y == 0 ? 1 : 0, connectionGrid[x, y].z, connectionGrid[x, y].w);
                break;
            default:
                break;
        }
    }
}