using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class RoomGeneratorEditor : EditorWindow {
    private Vector2Int size = new(1, 1);

    private Dictionary<Vector2Int, TileType> grid = new();
    private Dictionary<Vector2Int, float> heightGrid = new();
    private Dictionary<Vector2Int, Vector4> connectionGrid = new();

    private List<EnemyTeamSO> enemyTeams = new();

    private string tileName = "New Room";

    private Vector2Int lastSize = new();
    private Vector2 scrollPosition = Vector2.zero;

    private DungeonRoomSO currentRoom;
    private bool isHeight = true;
    private bool shouldDrawHeight = true;

    private int tilesPerRoom => GridStaticFunctions.TilesPerRoom;

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
            heightGrid = currentRoom.heightPositions.Zip(currentRoom.heightValues, (k, v) => new { k, v })
              .ToDictionary(x => x.k, x => x.v);

            enemyTeams = currentRoom.enemyTeams;
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

        if (size != lastSize) {
            AllocateRoomPositions();
            GenerateConnections();
            lastSize = size;
        }

        if (GUILayout.Button("Reset")) {
            AllocateRoomPositions();
            GenerateConnections();
        }

        if (GUILayout.Button("Generate Room"))
            GenerateRoom();

        if (connectionGrid.Count > 0)
            DrawConnections();

        if (grid.Count > 0) {
            DrawUILine(Color.gray);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = isHeight ? Color.green : Color.white;
            if (GUILayout.Button("HeightMap")) {
                isHeight = true;
                shouldDrawHeight = true;
            }

            GUI.backgroundColor = !isHeight ? Color.green : Color.white;
            if (GUILayout.Button("TileMap")) {
                isHeight = false;
                shouldDrawHeight = false;
            }
            EditorGUILayout.EndHorizontal();

            DrawUILine(Color.gray);

            if (shouldDrawHeight)
                DrawHeight();
            else
                DrawTiles();
        }

        if (grid.Select(x => x.Value).Where(x => x == TileType.EnemySpawn).Any())
            DrawEnemySelection();
    }

    private void GenerateRoom() {
        EditorSceneManager.OpenScene("Assets/Scenes/RoomEditor.unity", OpenSceneMode.Single);

        GameObject room = GameObject.Find("New Room");
        if (room != null) {
            for (int i = room.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(room.transform.GetChild(0).gameObject);
        }
        else {
            room = new GameObject("New Room");
            room.AddComponent<RoomComponent>();
        }

        Transform parent = room.transform;
        RoomComponent roomComp = room.GetComponent<RoomComponent>();

        roomComp.EnemyTeams = enemyTeams;

        Dictionary<Vector2Int, Tile> dict = GenerateGrid(parent);
        roomComp.Editor_SetUp(dict.Keys.ToList(), dict.Values.ToList(), heightGrid.Values.ToList());
    }

    private Dictionary<Vector2Int, Tile> GenerateGrid(Transform parent) {
        Dictionary<Vector2Int, Tile> result = new();
        Dictionary<TileType, Tile> prefabs = LoadPrefabs();

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                Vector2Int gridPos = new(x, y);
                Vector2Int connectionPos = new(Mathf.Min(Mathf.FloorToInt(x / tilesPerRoom), size.x - 1), Mathf.Min(Mathf.FloorToInt(y / tilesPerRoom), size.y - 1));

                Tile pref = prefabs[grid[new(x, y)]];
                if ((y == 0 && connectionGrid[connectionPos].w != GridStaticFunctions.CONST_INT) ||
                    (y == ((size.y * tilesPerRoom) - 1) && connectionGrid[connectionPos].z != GridStaticFunctions.CONST_INT) ||
                    (x == 0 && connectionGrid[connectionPos].y != GridStaticFunctions.CONST_INT) ||
                    (x == ((size.x * tilesPerRoom) - 1) && connectionGrid[connectionPos].x != GridStaticFunctions.CONST_INT)) {
                    if (x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) ||
                        x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) + 1 ||
                        x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) - 1 ||
                        y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) ||
                        y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) + 1 ||
                        y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2) - 1) {
                        pref = prefabs[TileType.Normal];
                    }
                }

                Tile tmp = Instantiate(pref, parent);
                if (y >= 1 && x >= 1 && y != size.y * tilesPerRoom - 1 && x != size.x * tilesPerRoom - 1 && grid[new(x, y)] == TileType.Normal)
                    tmp.transform.GetChild(0).eulerAngles = new Vector3(Random.Range(0, 4), Random.Range(0, 4), Random.Range(0, 4)) * 90;

                tmp.name = $"{gridPos} | {tmp.name}";
                tmp.SetHighlight(HighlightType.None);
                tmp.GridPos = gridPos;
                tmp.StandardWorldPosition = CalcSquareWorldPos(gridPos);
                tmp.transform.position = CalcSquareWorldPos(gridPos);
                tmp.transform.SetParent(parent);
                tmp.Height = heightGrid[gridPos];

                result.Add(gridPos, tmp);
            }
        }

        return result;
    }

    private Dictionary<TileType, Tile> LoadPrefabs() {
        Dictionary<TileType, Tile> result = new();

        GameObject hexGO = Resources.Load("GridBlocks/GridBlock") as GameObject;
        GameObject coverGO = Resources.Load("GridBlocks/GridCoverBlock") as GameObject;
        GameObject halfCoverGO = Resources.Load("GridBlocks/GridCoverBlock") as GameObject;
        GameObject lavaGO = Resources.Load("GridBlocks/GridLavaBlock") as GameObject;
        GameObject spawnGO = Resources.Load("GridBlocks/GridSpawnBlock") as GameObject;
        GameObject specialGO = Resources.Load("GridBlocks/GridSpecialBlock") as GameObject;

        Tile hex = hexGO.GetComponent<Tile>();
        Tile coverHex = coverGO.GetComponent<Tile>();
        Tile halfCoverHex = halfCoverGO.GetComponent<Tile>();
        Tile lavaHex = lavaGO.GetComponent<Tile>();
        Tile spawnHex = spawnGO.GetComponent<Tile>();
        Tile specialHex = specialGO.GetComponent<Tile>();

        result.Add(TileType.Normal, hex);
        result.Add(TileType.Cover, coverHex);
        result.Add(TileType.HalfCover, halfCoverHex);
        result.Add(TileType.Lava, lavaHex);
        result.Add(TileType.Spawn, spawnHex);
        result.Add(TileType.Special, specialHex);
        result.Add(TileType.EnemySpawn, hex);
        result.Add(TileType.BossSpawn, hex);

        return result;
    }

    private void SaveData() {
        DungeonRoomSO newScriptableObject = CreateInstance<DungeonRoomSO>();

        List<Vector4> list = new();
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++)
                list.Add(connectionGrid[new(x, y)]);
        }
        newScriptableObject.room = new(tileName, size, null, list, 0);

        newScriptableObject.gridPositions = new(grid.Keys);
        newScriptableObject.gridValues = new(grid.Values);

        newScriptableObject.connectionPositions = new(connectionGrid.Keys);
        newScriptableObject.connectionValues = new(connectionGrid.Values);

        newScriptableObject.heightPositions = new(heightGrid.Keys);
        newScriptableObject.heightValues = new(heightGrid.Values);

        newScriptableObject.enemyTeams = new(enemyTeams);

        AssetDatabase.CreateAsset(newScriptableObject, $"Assets/Resources/Rooms/{tileName}.asset");
        AssetDatabase.SaveAssets();

        currentRoom = null;
    }

    private Vector3 CalcSquareWorldPos(Vector2Int gridpos) {
        Vector2Int heightPos = gridpos;
        if (!heightGrid.ContainsKey(heightPos)) {
            if (heightPos.x < 0)
                heightPos.x = 0;
            if (heightPos.y < 0)
                heightPos.y = 0;
            if (heightPos.x >= size.x * tilesPerRoom)
                heightPos.x = (size.x * tilesPerRoom) - 1;
            if (heightPos.y >= size.y * tilesPerRoom)
                heightPos.y = (size.y * tilesPerRoom) - 1;
        }

        float x = gridpos.x - (((float)(size.x * tilesPerRoom) - 1) / 2);
        float y = heightGrid[heightPos];
        float z = gridpos.y - (((float)(size.y * tilesPerRoom) - 1) / 2);

        return new(x, y, z);
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
            GUI.backgroundColor = connectionGrid[new(x, size.y - 1)].z != GridStaticFunctions.CONST_INT ? Color.green : Color.white;

            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                ToggleConnectionState(x, size.y - 1, Directions.North);
        }
        EditorGUILayout.EndHorizontal();

        // Middle Line
        for (int y = size.y - 1; y >= 0; y--) {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < size.x + 2; x++) {
                if (x == 0) {
                    GUI.backgroundColor = connectionGrid[new(x, y)].y != GridStaticFunctions.CONST_INT ? Color.green : Color.white;

                    if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                        ToggleConnectionState(x, y, Directions.West);
                }
                else if (x == size.x + 1) {
                    GUI.backgroundColor = connectionGrid[new(x - 2, y)].x != GridStaticFunctions.CONST_INT ? Color.green : Color.white;

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
            GUI.backgroundColor = connectionGrid[new(x, 0)].w != GridStaticFunctions.CONST_INT ? Color.green : Color.white;

            if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                ToggleConnectionState(x, 0, Directions.South);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawTiles() {
        GUILayout.Label("Click on the buttons to identify blockades");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical();
        for (int y = (size.y * tilesPerRoom) - 2; y >= 1; y--) {
            EditorGUILayout.BeginHorizontal();

            for (int x = 1; x < (size.x * tilesPerRoom) - 1; x++) {
                HexTypeColor(grid[new(x, y)]);

                if (GUILayout.Button("  ", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
                    ToggleTileState(x, y);

                if (x % tilesPerRoom == tilesPerRoom - 1 && x < (size.x * tilesPerRoom) - 1)
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

    private void DrawHeight() {
        GUILayout.Label("Click on the buttons to identify Heights");
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.BeginVertical();
        for (int y = (size.y * tilesPerRoom) - 1; y >= 0; y--) {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                HexTypeColor(grid[new(x, y)]);

                heightGrid[new(x, y)] = EditorGUILayout.FloatField(heightGrid[new(x, y)], GUILayout.Width(18f), GUILayout.ExpandHeight(false));

                if (x % tilesPerRoom == tilesPerRoom - 1 && x < (size.x * tilesPerRoom) - 1)
                    EditorGUILayout.Space(.1f, false);
            }

            EditorGUILayout.EndHorizontal();
            if (y % tilesPerRoom == 0 && y < size.y * tilesPerRoom)
                EditorGUILayout.Space();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        DrawUILine(Color.gray);
        UpdateHeightForConnections();
    }

    private void DrawEnemySelection() {
        EditorGUILayout.BeginVertical();

        int enemyAmount = grid.Select(x => x.Value).Where(x => x == TileType.EnemySpawn).Count();
        if (enemyAmount != enemyTeams.Count) {
            List<EnemyTeamSO> tmpEnemies = new(enemyTeams);

            enemyTeams.Clear();
            for (int i = 0; i < enemyAmount; i++)
                enemyTeams.Add(i < tmpEnemies.Count - 1 ? tmpEnemies[i] : null);
        }

        for (int i = 0; i < enemyAmount; i++)
            enemyTeams[i] = (EnemyTeamSO)EditorGUILayout.ObjectField($"Enemy team {i}", enemyTeams[i], typeof(EnemyTeamSO), false);

        EditorGUILayout.EndVertical();
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
        heightGrid.Clear();

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++) {
                if (x == 0 || y == 0 || x == (size.x * tilesPerRoom) - 1 || y == (size.y * tilesPerRoom) - 1)
                    grid.Add(new(x, y), TileType.Cover);
                else
                    grid.Add(new(x, y), TileType.Normal);
            }
        }

        for (int y = 0; y < size.y * tilesPerRoom; y++) {
            for (int x = 0; x < size.x * tilesPerRoom; x++)
                heightGrid.Add(new(x, y), 0);
        }
    }

    private void GenerateConnections() {
        connectionGrid.Clear();

        for (int y = 0; y < size.y; y++) {
            for (int x = 0; x < size.x; x++)
                connectionGrid.Add(new(x, y), new Vector4(GridStaticFunctions.CONST_INT, GridStaticFunctions.CONST_INT, GridStaticFunctions.CONST_INT, GridStaticFunctions.CONST_INT));
        }
    }

    private void ToggleTileState(int x, int y) {
        switch (grid[new(x, y)]) {
            case TileType.Normal:
                grid[new(x, y)] = TileType.HalfCover;
                break;
            case TileType.HalfCover:
                grid[new(x, y)] = TileType.Cover;
                break;
            case TileType.Cover:
                grid[new(x, y)] = TileType.Lava;
                break;
            case TileType.Lava:
                grid[new(x, y)] = TileType.Spawn;
                break;
            case TileType.Spawn:
                grid[new(x, y)] = TileType.Special;
                break;
            case TileType.Special:
                grid[new(x, y)] = TileType.EnemySpawn;
                break;
            case TileType.EnemySpawn:
                grid[new(x, y)] = TileType.BossSpawn;
                break;
            case TileType.BossSpawn:
                grid[new(x, y)] = TileType.Normal;
                break;

            default:
                break;
        }
    }

    private void ToggleConnectionState(int x, int y, Directions direction) {
        Vector4 connection = connectionGrid[new(x, y)];

        switch (direction) {
            case Directions.North:
                connectionGrid[new(x, y)] = new(connection.x, connection.y, connection.z == GridStaticFunctions.CONST_INT ? 0 : GridStaticFunctions.CONST_INT, connection.w);
                break;
            case Directions.East:
                connectionGrid[new(x, y)] = new(connection.x == GridStaticFunctions.CONST_INT ? 0 : GridStaticFunctions.CONST_INT, connection.y, connection.z, connection.w);
                break;
            case Directions.South:
                connectionGrid[new(x, y)] = new(connection.x, connection.y, connection.z, connection.w == GridStaticFunctions.CONST_INT ? 0 : GridStaticFunctions.CONST_INT);
                break;
            case Directions.West:
                connectionGrid[new(x, y)] = new(connection.x, connection.y == GridStaticFunctions.CONST_INT ? 0 : GridStaticFunctions.CONST_INT, connection.z, connection.w);
                break;
            default:
                break;
        }

        UpdateHeightForConnections();
    }

    private void UpdateHeightForConnections() {
        for (int y = -1; y < size.y * tilesPerRoom + 1; y++) {
            for (int x = -1; x < size.x * tilesPerRoom + 1; x++) {
                Vector2Int gridPos = new(x, y);
                Vector2Int connectionPos = new(Mathf.Min(Mathf.FloorToInt(x / tilesPerRoom), size.x - 1), Mathf.Min(Mathf.FloorToInt(y / tilesPerRoom), size.y - 1));

                Vector2Int heightPos = gridPos;
                if (!heightGrid.ContainsKey(heightPos)) {
                    if (heightPos.x < 0)
                        heightPos.x = 0;
                    if (heightPos.y < 0)
                        heightPos.y = 0;
                    if (heightPos.x >= size.x * tilesPerRoom)
                        heightPos.x = (size.x * tilesPerRoom) - 1;
                    if (heightPos.y >= size.y * tilesPerRoom)
                        heightPos.y = (size.y * tilesPerRoom) - 1;
                }

                if (x == size.x * tilesPerRoom && connectionGrid[connectionPos].x != GridStaticFunctions.CONST_INT) {
                    if (y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2))
                        connectionGrid[connectionPos] = new Vector4(heightGrid[heightPos], connectionGrid[connectionPos].y, connectionGrid[connectionPos].z, connectionGrid[connectionPos].w);
                }
                if (x == -1 && connectionGrid[connectionPos].y != GridStaticFunctions.CONST_INT) {
                    if (y % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2))
                        connectionGrid[connectionPos] = new Vector4(connectionGrid[connectionPos].x, heightGrid[heightPos], connectionGrid[connectionPos].z, connectionGrid[connectionPos].w);
                }

                if (y == size.y * tilesPerRoom && connectionGrid[connectionPos].z != GridStaticFunctions.CONST_INT) {
                    if (x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2))
                        connectionGrid[connectionPos] = new Vector4(connectionGrid[connectionPos].x, connectionGrid[connectionPos].y, heightGrid[heightPos], connectionGrid[connectionPos].w);
                }
                if (y == -1 && connectionGrid[connectionPos].w != GridStaticFunctions.CONST_INT) {
                    if (x % tilesPerRoom == Mathf.RoundToInt((tilesPerRoom - 1) / 2))
                        connectionGrid[connectionPos] = new Vector4(connectionGrid[connectionPos].x, connectionGrid[connectionPos].y, connectionGrid[connectionPos].z, heightGrid[heightPos]);
                }
            }
        }
    }

    private void HexTypeColor(TileType type) {
        switch (type) {
            case TileType.Normal:
                GUI.backgroundColor = Color.white;
                break;
            case TileType.Lava:
                GUI.backgroundColor = new Color(1f, .5f, 0);
                break;
            case TileType.Cover:
                GUI.backgroundColor = Color.black;
                break;
            case TileType.HalfCover:
                GUI.backgroundColor = Color.gray;
                break;
            case TileType.Spawn:
                GUI.backgroundColor = Color.green;
                break;
            case TileType.Special:
                GUI.backgroundColor = Color.yellow;
                break;
            case TileType.EnemySpawn:
                GUI.backgroundColor = Color.red;
                break;
            case TileType.BossSpawn:
                GUI.backgroundColor = new Color(.5f, 0, 0);
                break;

            default:
                break;
        }
    }
}