using UnityEditor;
using UnityEngine;

// We are taking some liberties, Knowing the child counts
// This is also very hard coded for the simple reason as it is faster and cleaner.
// This also does not need to be fast, which is nice.

public class RoomUpdater : EditorWindow {
    private RoomComponent RoomToFix = null;

    [MenuItem("Custom/Room Updater")]
    public static void ShowWindow() {
        GetWindow(typeof(RoomUpdater));
    }

    private void OnGUI() {
        if (GUILayout.Button("Fix all")) {

        }

        DrawUILine(Color.gray);

        RoomToFix = (RoomComponent)EditorGUILayout.ObjectField("Room To Fix", RoomToFix, typeof(RoomComponent), false);
        if (RoomToFix != null)
            if (GUILayout.Button("Fix")) {
                Debug.Log($"Fixing {RoomToFix.gridValues.Count} tiles");

                RemoveUnneededObjects();
                SetProperLayers();
            }

        EditorGUILayout.HelpBox("Put a room into the input field to update it.", MessageType.Info);
    }

    private void RemoveUnneededObjects() {
        for (int i = 0; i < RoomToFix.gridValues.Count; i++) {
            Tile roomTile = RoomToFix.gridValues[i];

            if (roomTile.Type is not TileType.Cover or TileType.HalfCover)
                continue;

            for (int child = roomTile.transform.childCount - 1; child >= 0; child--) {
                Transform tileChild = roomTile.transform.GetChild(child);

                for (int child2 = tileChild.childCount - 1; child2 >= 0; child2--) {
                    Transform tileChild2 = tileChild.transform.GetChild(child2);

                    if (tileChild2.gameObject.layer == 6 || tileChild2.transform.childCount > 0)
                        continue;

                    DestroyImmediate(tileChild2.gameObject, true);
                }
            }
        }
    }

    private void SetProperLayers() {
        for (int i = 0; i < RoomToFix.gridValues.Count; i++) {
            Tile roomTile = RoomToFix.gridValues[i];

            if (roomTile.Type is not TileType.Cover or TileType.HalfCover) {
                if (roomTile.Type is TileType.Lava)
                    continue;

                roomTile.tag = "WalkableTile";

                for (int child = roomTile.transform.childCount - 1; child >= 0; child--) {
                    Transform tileChild = roomTile.transform.GetChild(child);
                    tileChild.tag = "WalkableTile";

                    for (int child2 = tileChild.childCount - 1; child2 >= 0; child2--) {
                        Transform tileChild2 = tileChild.transform.GetChild(child2);

                        tileChild2.tag = "WalkableTile";
                    }
                }
            }
            else {
                roomTile.tag = "Untagged";

                for (int child = roomTile.transform.childCount - 1; child >= 0; child--) {
                    Transform tileChild = roomTile.transform.GetChild(child);
                    tileChild.tag = "Untagged";

                    for (int child2 = tileChild.childCount - 1; child2 >= 0; child2--) {
                        Transform tileChild2 = tileChild.transform.GetChild(child2);

                        tileChild2.tag = "Untagged";
                        tileChild2.gameObject.layer = 6;
                    }
                }
            }
        }
    }

    private void DrawUILine(Color color, int thickness = 1, int padding = 10) {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }
}
