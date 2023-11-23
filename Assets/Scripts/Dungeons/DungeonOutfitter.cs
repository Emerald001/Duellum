using UnityEngine;

public class DungeonOutfitter : MonoBehaviour {
    public void OutfitDungeon() {
        foreach (var room in GridStaticFunctions.Dungeon) {
            Vector2Int gridpos = room.Key;
            RoomComponent roomComp = room.Value;

            if (gridpos == roomComp.indexZeroGridPos) {
                OverwriteGridPositions(roomComp);
                roomComp.SetUp(gridpos * GridStaticFunctions.TilesPerRoom);
            }
        }
    }

    private void OverwriteGridPositions(RoomComponent room) {
        switch (room.rotationIndex) {
            case 1:
                int index1 = 0;
                for (int x = 0; x < (room.size.x * GridStaticFunctions.TilesPerRoom); x++) {
                    for (int y = room.size.y * GridStaticFunctions.TilesPerRoom - 1; y >= 0; y--) {
                        room.gridPositions[index1] = new(x, y);
                        index1++;
                    }
                }
                break;
            case 2:
                int index2 = 0;
                for (int y = (room.size.y * GridStaticFunctions.TilesPerRoom) - 1; y >= 0; y--) {
                    for (int x = (room.size.x * GridStaticFunctions.TilesPerRoom) - 1; x >= 0; x--) {
                        room.gridPositions[index2] = new(x, y);
                        index2++;
                    }
                }
                break;
            case 3:
                int index3 = 0;
                for (int x = room.size.x * GridStaticFunctions.TilesPerRoom - 1; x >= 0; x--) {
                    for (int y = 0; y < room.size.y * GridStaticFunctions.TilesPerRoom; y++) {
                        room.gridPositions[index3] = new(x, y);
                        index3++;
                    }
                }
                break;

            default:
                return;
        }
    }
}
