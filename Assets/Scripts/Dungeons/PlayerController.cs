using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject visuals;
    [SerializeField] private float moveSpeed;

    public Vector2Int PlayerPosition => playerPosition;

    private ActionQueue actionQueue;

    private readonly Dictionary<Vector2Int, Vector2Int> parentDictionary = new();
    private readonly List<Vector2Int> currentAccessableTiles = new();

    private Vector2Int playerPosition;
    private Vector2Int preBattlePos;

    private bool isWalking;
    private bool inBattle;

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartPlayerStartSequence, (x) => StartCoroutine(OnBattleStart(x)));
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, OnBattleEnd);
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartPlayerStartSequence, (x) => StartCoroutine(OnBattleStart(x)));
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, OnBattleEnd);
    }

    public void SetUp(Vector2Int gridPosition) {
        actionQueue = new(() => isWalking = false);
        playerPosition = gridPosition;

        FindAccessibleTiles(playerPosition, 30);
    }

    private void Update() {
        if (inBattle)
            return;

        actionQueue.OnUpdate();

        if (MouseToWorldView.HoverTileGridPos == GridStaticFunctions.CONST_EMPTY ||
            !currentAccessableTiles.Contains(MouseToWorldView.HoverTileGridPos)) {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
            line.enabled = false;
            return;
        }

        if (isWalking) {
            line.enabled = false;
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
            return;
        }
        else {
            DrawPathWithLine(GetPath(MouseToWorldView.HoverTileGridPos));
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Move);

            if (Input.GetKeyDown(KeyCode.Mouse0))
                Move();
        }
    }

    private IEnumerator OnBattleStart(BattleData data) {
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);

        if (isWalking)
            actionQueue.Clear();

        visuals.SetActive(false);
        line.enabled = false;
        inBattle = true;

        Vector2Int difference = data.PlayerPos - data.EnemyPos;
        Vector2Int middlePoint = data.PlayerPos + -difference / 2;

        Vector3 newPos = GridStaticFunctions.CalcWorldPos(data.EnemyPos) + new Vector3(0, -2, 0);
        while (transform.position != newPos) {
            transform.position = Vector3.MoveTowards(transform.position, newPos, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        newPos = GridStaticFunctions.CalcWorldPos(middlePoint) + new Vector3(0, -2, 0);
        while (transform.position != newPos) {
            transform.position = Vector3.MoveTowards(transform.position, newPos, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnBattleEnd() {
        transform.position = GridStaticFunctions.CalcWorldPos(preBattlePos);
        playerPosition = preBattlePos;

        visuals.SetActive(true);
        inBattle = false;

        FindAccessibleTiles(playerPosition, 30);
    }

    private void Move() {
        Vector2Int position = MouseToWorldView.HoverTileGridPos;
        if (position == GridStaticFunctions.CONST_EMPTY)
            return;

        EnqueueMovement(position);
        isWalking = true;
    }

    private void EnqueueMovement(Vector2Int targetPosition) {
        actionQueue.Enqueue(new DoMethodAction(() => {
            //UnitAnimator.WalkAnim(true);
            //UnitAudio.PlayLoopedAudio("Walking", true);
        }));

        Vector2Int lastPos = playerPosition;
        foreach (var newPos in GetPath(targetPosition)) {
            Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcWorldPos(newPos) - GridStaticFunctions.CalcWorldPos(lastPos));

            actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, GridStaticFunctions.CalcWorldPos(newPos)));
            actionQueue.Enqueue(new DoMethodAction(() => {
                playerPosition = newPos;
            }));

            lastPos = newPos;
        }

        actionQueue.Enqueue(new DoMethodAction(() => {
            //UnitAnimator.WalkAnim(false);
            //UnitAudio.PlayLoopedAudio("Walking", false);

            FindAccessibleTiles(playerPosition, 30);
            GridStaticFunctions.ResetAllTileColors();
        }));
    }

    public void FindAccessibleTiles(Vector2Int gridPos, int speedValue) {
        playerPosition = gridPos;

        parentDictionary.Clear();
        currentAccessableTiles.Clear();

        List<Vector2Int> openList = new();
        List<Vector2Int> layerList = new();
        List<Vector2Int> closedList = new();

        openList.Add(gridPos);
        for (int i = 0; i < speedValue; i++) {
            foreach (var currentPos in openList.ToList()) {
                GridStaticFunctions.RippleThroughFullGridPositions(currentPos, 2, (neighbour, count) => {
                    if (neighbour == currentPos)
                        return;

                    // Only applicable if no other thing is needed
                    if (GridStaticFunctions.Grid[neighbour].Type != TileType.Normal)
                        return;

                    if (openList.Contains(neighbour) ||
                        closedList.Contains(neighbour) ||
                        layerList.Contains(neighbour))
                        return;

                    layerList.Add(neighbour);
                    currentAccessableTiles.Add(neighbour);
                    parentDictionary[neighbour] = currentPos;
                });

                closedList.Add(currentPos);
            }

            openList.Clear();
            openList.AddRange(layerList);
            layerList.Clear();
        }
    }

    public List<Vector2Int> GetPath(Vector2Int endPos) {
        List<Vector2Int> path = new();
        Vector2Int currentPosition = endPos;

        while (currentPosition != playerPosition) {
            path.Add(currentPosition);
            currentPosition = parentDictionary[currentPosition];
        }

        path.Add(currentPosition);
        path.Reverse();

        return path;
    }

    private void DrawPathWithLine(List<Vector2Int> currentPath) {
        line.enabled = true;

        if (currentPath != null && currentPath.Count > 0) {
            line.positionCount = currentPath.Count + 1;
            for (int i = 0; i < currentPath.Count + 1; i++) {
                if (i == 0) {
                    line.SetPosition(0, GridStaticFunctions.CalcWorldPos(playerPosition) + new Vector3(0, .05f, 0));
                    continue;
                }
                line.SetPosition(i, GridStaticFunctions.CalcWorldPos(currentPath[i - 1]) + new Vector3(0, .05f, 0));
            }
        }
    }
}