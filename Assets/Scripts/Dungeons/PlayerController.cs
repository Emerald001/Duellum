using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject visuals;
    [SerializeField] private float moveSpeed;

    private ActionQueue actionQueue;

    private readonly Dictionary<Vector2Int, Vector2Int> parentDictionary = new();
    private readonly List<Vector2Int> currentAccessableTiles = new();
    private Vector2Int playerPosition = new(6, 6);

    private Vector2Int preBattlePos;

    private bool isWalking;
    private bool inBattle;

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartBattle, OnBattleStart);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, OnBattleEnd);
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartBattle, OnBattleStart);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, OnBattleEnd);
    }

    private void Start() {
        actionQueue = new(() => isWalking = false);

        FindAccessibleTiles(playerPosition, 30);
    }

    private void Update() {
        if (inBattle)
            return;

        actionQueue.OnUpdate();

        if (MouseToWorldView.HoverTileGridPos == GridStaticFunctions.CONST_EMPTY ||
            !currentAccessableTiles.Contains(MouseToWorldView.HoverTileGridPos)) {
            line.enabled = false;
            return;
        }

        if (isWalking) {
            line.enabled = false;
            return;
        }
        else {
            DrawPathWithLine(GetPath(MouseToWorldView.HoverTileGridPos));
            if (Input.GetKeyDown(KeyCode.Mouse0))
                Move();
        }
    }

    private void OnBattleStart(BattleData data) {
        if (isWalking)
            actionQueue.Clear();

        preBattlePos = playerPosition;

        Vector2Int difference = data.PlayerPos - data.EnemyPos;
        Vector2Int middlePoint = data.PlayerPos + difference / 2;

        transform.position = GridStaticFunctions.CalcDungeonTileWorldPos(middlePoint);

        visuals.SetActive(false);
        line.enabled = false;

        inBattle = true;
    }

    private void OnBattleEnd() {
        transform.position = GridStaticFunctions.CalcDungeonTileWorldPos(preBattlePos);
        playerPosition = preBattlePos;

        visuals.SetActive(false);
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
            Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcSquareWorldPos(newPos) - GridStaticFunctions.CalcSquareWorldPos(lastPos));

            actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, GridStaticFunctions.CalcDungeonTileWorldPos(newPos)));
            actionQueue.Enqueue(new DoMethodAction(() => {
                playerPosition = newPos;
            }));

            lastPos = newPos;
        }

        actionQueue.Enqueue(new DoMethodAction(() => {
            //UnitAnimator.WalkAnim(false);
            //UnitAudio.PlayLoopedAudio("Walking", false);

            FindAccessibleTiles(playerPosition, 30);
            GridStaticFunctions.ResetTileColors();
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
                GridStaticFunctions.RippleThroughFullSquareGridPositions(currentPos, 2, (neighbour, count) => {
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
                    line.SetPosition(0, GridStaticFunctions.CalcDungeonTileWorldPos(playerPosition));
                    continue;
                }
                line.SetPosition(i, GridStaticFunctions.CalcDungeonTileWorldPos(currentPath[i - 1]));
            }
        }
    }
}