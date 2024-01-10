using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleSequences : MonoBehaviour {
    [SerializeField] private BattleManager battleManager;

    private ActionQueue actionQueue = new();
    private int BattleMapSize => GridStaticFunctions.BattleMapSize;

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartBattle, SetStartSequence);
        EventManager<BattleEvents, Vector2Int>.Subscribe(BattleEvents.BattleEnd, SetEndSequence);
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartBattle, SetStartSequence);
        EventManager<BattleEvents, Vector2Int>.Unsubscribe(BattleEvents.BattleEnd, SetEndSequence);
    }

    private void Update() {
        actionQueue.OnUpdate();
    }

    private void SetStartSequence(BattleData data) {
        Vector2Int difference = data.PlayerPos - data.EnemyPos;
        Vector2Int middlePoint = data.PlayerPos - difference / 2;
        Vector2Int direction = Mathf.Abs(difference.x) > Mathf.Abs(difference.y) ? new(0, 1) : new(1, 0);

        List<Vector2Int> points = new();
        for (int x = -((BattleMapSize - 1) / 2); x <= ((BattleMapSize - 1) / 2); x++) {
            for (int y = -((BattleMapSize - 1) / 2); y <= ((BattleMapSize - 1) / 2); y++)
                points.Add(middlePoint + new Vector2Int(x, y));
        }

        GridStaticFunctions.SetBattleGrid(points);
        battleManager.SetBattlefield(data, direction);

        // Do Audio Thing actionQueue.Enqueue(new DoMethodAction(() => ));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation, "Battle Started")));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<BattleEvents, BattleData>.Invoke(BattleEvents.StartPlayerStartSequence, data)));
        actionQueue.Enqueue(new WaitAction(.5f));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<CameraEventType, float>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, 10)));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<CameraEventType, Transform>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, GridStaticFunctions.Grid[data.EnemyPos].transform)));
        actionQueue.Enqueue(new WaitAction(1f));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<CameraEventType, Transform>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, GridStaticFunctions.Grid[middlePoint].transform)));
        actionQueue.Enqueue(new WaitAction(1f));
        actionQueue.Enqueue(new DoMethodAction(() => Ripple(middlePoint, 3, points)));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<CameraEventType, EventMessage<float, float>>.Invoke(CameraEventType.ZOOM_CAM, new(3, 10))));
        actionQueue.Enqueue(new WaitAction(.2f));
        actionQueue.Enqueue(new DoMethodAction(() => EventManager<CameraEventType, EventMessage<float, float>>.Invoke(CameraEventType.ZOOM_CAM, new(7, 10))));
        actionQueue.Enqueue(new WaitAction(.2f));
        actionQueue.Enqueue(new DoMethodAction(() => battleManager.StartBattle(data)));
    }

    private void SetEndSequence(Vector2Int startPosition) {
        actionQueue.Enqueue(new DoMethodAction(() => EndRipple(startPosition, 3)));
    }

    private void Ripple(Vector2Int gridPos, float rippleStrength, List<Vector2Int> battleMap) {
        GridStaticFunctions.RippleThroughGridPositions(gridPos, 60, (gridPos, i) => {
            Tile currentHex = GridStaticFunctions.Grid[gridPos];
            currentHex.ClearQueue();

            StartCoroutine(currentHex.DoRipple(i / 50f, rippleStrength, !battleMap.Contains(gridPos)));
        });

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .4f);
    }

    private void EndRipple(Vector2Int gridPos, float rippleStrength) {
        GridStaticFunctions.RippleThroughGridPositions(gridPos, 100, (gridPos, i) => {
            Tile currentHex = GridStaticFunctions.Grid[gridPos];
            currentHex.ClearQueue();
            List<Action> queue = new() {
                new WaitAction(i / 50f),
                new MoveObjectAction(currentHex.gameObject, 30, currentHex.StandardWorldPosition - new Vector3(0, rippleStrength, 0)),
                new DoMethodAction(() => currentHex.transform.GetChild(0).gameObject.SetActive(true)),
                new MoveObjectAction(currentHex.gameObject, 20, currentHex.StandardWorldPosition + new Vector3(0, rippleStrength / 3, 0)),
                new MoveObjectAction(currentHex.gameObject, 10, currentHex.StandardWorldPosition - new Vector3(0, rippleStrength / 6, 0)),
                new MoveObjectAction(currentHex.gameObject, 5, currentHex.StandardWorldPosition + new Vector3(0, rippleStrength / 15, 0)),
                new MoveObjectAction(currentHex.gameObject, 5, currentHex.StandardWorldPosition),
            };

            currentHex.SetActionQueue(queue);
        });

        EventManager<CameraEventType, float>.Invoke(CameraEventType.DO_CAMERA_SHAKE, .4f);
    }
}
