using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class RoomGeneratorEditor : EditorWindow {
    private Vector2Int size = new(1, 1);

    private Dictionary<Vector2Int, bool> grid = new();
    private Dictionary<Vector2Int, Vector4> connectionGrid = new();

    private string tileName = "New Room";

    private Vector2Int lastSize = new();
    private Vector2 scrollPosition = Vector2.zero;
    private readonly int tilesPerRoom = 11;

    private DungeonRoomSO currentRoom;

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

        currentRoom = (DungeonRoomSO)EditorGUILayout.ObjectField("Current Room", currentRoom, typeof(DungeonRoomSO), false);

        if (GUILayout.Button("Save"))
            SaveData();

        if (GUILayout.Button("Load")) {
            tileName = currentRoom.room.name;
            size = currentRoom.room.size;
            
            grid = currentRoom.gridPositions.Zip(currentRoom.gridValues, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);
            connectionGrid = currentRoom.connectionPositions.Zip(currentRoom.connectionValues, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);

            DrawConnections();
            DrawTiles();
        }

        DrawUILine(Color.gray);

        tileName = EditorGUILayout.TextField("Name", tileName);
        size = EditorGUILayout.Vector2IntField("Size", size);

        DrawUILine(Color.gray);

        if (size.x > 10)
            size = new(10, size.y);
        if (size.y > 10)
            size = new(size.x, 10);

        if (size.x < 1)
            size = new(1, size.y);
        if (size.y < 1)
            size = new(size.x, 1);

        if (GUILayout.Button("Reset")) {
            AllocateRoomPositions();
            GenerateConnections();
        }

        if (GUILayout.Button("Generate Room"))
            GenerateRoom();

        if (size != lastSize) {
            AllocateRoomPositions();
            GenerateConnections();
            lastSize = size;
        }

        if (connectionGrid.Count > 0)
            DrawConnections();

        if (grid.Count > 0)
            DrawTiles();
    }

    private void GenerateRoom() {
        EditorSceneManager.OpenScene("Assets/Scenes/RoomEditor.unity", OpenSceneMode.Single);

        GameObject room = GameObject.Find("New Room");
        if (room != null) {
            for (int i = room.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(room.transform.GetChild(0).gameObject);
        }

        var comp = room.GetComponent<RoomComponent>();
        GameObject hexGO = Resources.Load("GridBlocks/GridBlock") as GameObject;
        GameObject coverGO = Resources.Load("GridBlocks/GridCoverBlock") as GameObject;

        Hex hex = hexGO.GetComponent<Hex>();
        Hex coverHex = coverGO.GetComponent<Hex>();

        Transform parent = room != null ? room.transform : new GameObject("New Room").transform;

        Dictionary<Vector2Int, Hex> dict = GenerateGrid(parent, hex, coverHex);
        comp.SetUp(dict.Keys.ToList(), dict.Values.ToList());
    }

    private Dictionary<Vector2Int, Hex> GenerateGrid(Transform parent, Hex prefab, Hex coverPrefab) {
        Dictionary<Vector2Int, Hex> result = new();

        for (int y = -1; y < size.y * tilesPerRoom + 1; y++) {
            for (int x = -1; x < size.x * tilesPerRoom + 1; x++) {
                Vector2Int gridPos = new(x, y);
                Vector2Int connectionPos = new(Mathf.Min(Mathf.FloorToInt(x / tilesPerRoom), size.x - 1), Mathf.Min(Mathf.FloorToInt(y / tilesPerRoom), size.y - 1));

                Hex pref = y < 0 || x < 0 || y == size.y * tilesPerRoom || x == size.x * tilesPerRoom
                    ? coverPrefab
                    : grid[new(x, y)] ? prefab : coverPrefab;

                if ((y == -1 && connectionGrid[connectionPos].w == 1) ||
                    (y == size.y * tilesPerRoom && connectionGrid[connectionPos].z == 1) ||
                    (x == -1 && connectionGrid[connectionPos].y == 1) ||
                    (x == size.x * tilesPerRoom && connectionGrid[connectionPos].x == 1)) {
                    if (x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) ||
                        x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) + 1 ||
                        x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) - 1 ||
                        y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) ||
                        y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) + 1 ||
                        y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) - 1) {
                        pref = prefab;
                    }
                }

                Hex tmp = Instantiate(pref, parent);
                if (y >= 0 && x >= 0 && y != size.y * tilesPerRoom && x != size.x * tilesPerRoom && grid[new(x, y)])
                    tmp.transform.GetChild(0).eulerAngles = new Vector3(Random.Range(0, 4), Random.Range(0, 4), Random.Range(0, 4)) * 90;

                tmp.name = $"{gridPos} | {tmp.name}";
                tmp.SetHighlight(HighlightType.None);
                tmp.GridPos = gridPos;
                tmp.StandardWorldPosition = CalcSquareWorldPos(gridPos);
                tmp.transform.position = CalcSquareWorldPos(gridPos);
                tmp.transform.SetParent(parent);

                result.Add(gridPos, tmp);
            }
        }

        return result;
    }

    private void SaveData() {
        DungeonRoomSO newScriptableObject = CreateInstance<DungeonRoomSO>();

        List<Vector4> list = new();
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++)
                list.Add(connectionGrid[new(x, y)]);
        }
        newScriptableObject.room = new(tileName, size, null, list);

        newScriptableObject.gridPositions = new(grid.Keys);
        newScriptableObject.gridValues = new(grid.Values);

        newScriptableObject.connectionPositions = new(connectionGrid.Keys);
        newScriptableObject.connectionValues = new(connectionGrid.Values);

        AssetDatabase.CreateAsset(newScriptableObject, $"Assets/Resources/Rooms/{tileName}.asset");
        AssetDatabase.SaveAssets();

        currentRoom = null;
    }

    private Vector3 CalcSquareWorldPos(Vector2Int gridpos) {
        float x = gridpos.x - (((size.x * tilesPerRoom) - 1 + (0 * ((size.x * tilesPerRoom) - 1))) / 2) + 0 * gridpos.x;
        float z = gridpos.y - (((size.y * tilesPerRoom) - 1 + (0 * ((size.y * tilesPerRoom) - 1))) / 2) + 0 * gridpos.y;

        return new Vector3(x, 0, z);
    }
}

public partial class RoomGeneratorEditor {
    private void DrawConnections() {
        GUILayout.Label("Click on the buttons to identify their connections");

        EditorGUILayout.BeginVertical();

        // First Line
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(21f, false);
        for (int x = 0; x < size.x; x++) {
            GUI.backgroundColor = connectionGrid[new(x, size.y - 1)].z == 1 ? Color.green : Color.white;

            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                ToggleConnectionState(x, size.y - 1, Directions.North);
        }
        EditorGUILayout.EndHorizontal();

        // Middle Line
        for (int y = size.y - 1; y >= 0; y--) {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < size.x + 2; x++) {
                if (x == 0) {
                    GUI.backgroundColor = connectionGrid[new(x, y)].y == 1 ? Color.green : Color.white;

                    if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                        ToggleConnectionState(x, y, Directions.West);
                }
                else if (x == size.x + 1) {
                    GUI.backgroundColor = connectionGrid[new(x - 2, y)].x == 1 ? Color.green : Color.white;

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
            GUI.backgroundColor = connectionGrid[new(x, 0)].w == 1 ? Color.green : Color.white;

            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                ToggleConnectionState(x, 0, Directions.South);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawTiles() {
        DrawUILine(Color.gray);

        GUILayout.Label("Click on the buttons to identify blockades");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical();
        for (int y = size.y * tilesPerRoom - 1; y >= 0; y--) {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                GUI.backgroundColor = grid[new(x, y)] ? Color.white : Color.gray;

                if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                    ToggleTileState(x, y);

                if (x % tilesPerRoom == tilesPerRoom - 1 && x < size.x * tilesPerRoom - 1)
                    EditorGUILayout.Space(.1f, false);
            }

            EditorGUILayout.EndHorizontal();
            if (y % tilesPerRoom == 0 && y < size.y * tilesPerRoom)
                EditorGUILayout.Space();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

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

    private void AllocateRoomPositions() {
        grid.Clear();

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++)
                grid.Add(new(x, y), true);
        }
    }

    private void GenerateConnections() {
        connectionGrid.Clear();

        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++)
                connectionGrid.Add(new(x, y), new Vector4());
        }
    }

    private void ToggleTileState(int x, int y) {
        grid[new(x, y)] = !grid[new(x, y)];
    }

    private void ToggleConnectionState(int x, int y, Directions direction) {
        switch (direction) {
            case Directions.North:
                connectionGrid[new(x, y)] = new(connectionGrid[new(x, y)].x, connectionGrid[new(x, y)].y, connectionGrid[new(x, y)].z == 0 ? 1 : 0, connectionGrid[new(x, y)].w);
                break;
            case Directions.East:
                connectionGrid[new(x, y)] = new(connectionGrid[new(x, y)].x == 0 ? 1 : 0, connectionGrid[new(x, y)].y, connectionGrid[new(x, y)].z, connectionGrid[new(x, y)].w);
                break;
            case Directions.South:
                connectionGrid[new(x, y)] = new(connectionGrid[new(x, y)].x, connectionGrid[new(x, y)].y, connectionGrid[new(x, y)].z, connectionGrid[new(x, y)].w == 0 ? 1 : 0);
                break;
            case Directions.West:
                connectionGrid[new(x, y)] = new(connectionGrid[new(x, y)].x, connectionGrid[new(x, y)].y == 0 ? 1 : 0, connectionGrid[new(x, y)].z, connectionGrid[new(x, y)].w);
                break;
            default:
                break;
        }
    }
}