using UnityEditor;
using UnityEngine;

public class QuickFixer : EditorWindow {
    private RoomComponent RoomToFix = null;

    [MenuItem("Custom/Room Fixer")]
    public static void ShowWindow() {
        GetWindow(typeof(QuickFixer));
    }

    private void OnGUI() {
        RoomToFix = (RoomComponent)EditorGUILayout.ObjectField("Room To Fix", RoomToFix, typeof(RoomComponent), false);

        if (RoomToFix == null)
            return;

        if (GUILayout.Button("Fix")) {
            Debug.Log($"Fixing {RoomToFix.gridValues.Count} tiles");

            for (int i = 0; i < RoomToFix.gridValues.Count; i++) {
                Tile roomTile = RoomToFix.gridValues[i];

                Debug.Log($"Checking {roomTile.name}");

                if (roomTile.Type is not TileType.Cover or TileType.HalfCover)
                    continue;

                Debug.Log($"{roomTile.name} is of type Cover");

                Debug.Log($"{roomTile.name} has {roomTile.transform.childCount} children");
                for (int child = roomTile.transform.childCount - 1; child >= 0; child--) {
                    Transform tileChild = roomTile.transform.GetChild(child);

                    for (int child2 = tileChild.childCount - 1; child2 >= 0; child2--) {
                        Transform tileChild2 = tileChild.transform.GetChild(child2);

                        if (tileChild2.gameObject.layer == 6 || tileChild2.transform.childCount > 0)
                            continue;

                        Debug.Log($"Destroying {tileChild2.name}");

                        DestroyImmediate(tileChild2.gameObject, true);
                    }
                }
            }
        }
    }
}
