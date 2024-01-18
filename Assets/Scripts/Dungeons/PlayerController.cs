using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private LineRenderer line;
    [SerializeField] private GameObject visuals;
    [SerializeField] private FootstepManager footstepManager;
    [SerializeField] private Animator unitAnimator;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotSpeed;

    public Vector2Int PlayerPosition => playerPosition;

    private readonly Dictionary<Vector2Int, Vector2Int> parentDictionary = new();
    private readonly List<Vector2Int> currentAccessableTiles = new();

    private readonly HashSet<Vector2Int> openSet = new();
    private readonly HashSet<Vector2Int> closedSet = new();

    private readonly Stack<bool> interactableStack = new();

    private ActionQueue actionQueue;

    private Vector2Int playerPosition;
    private Vector2Int lookDirection;

    private bool isWalking;
    private bool IsInteractable => interactableStack.Count < 1;

    private void OnEnable() {
        EventManager<BattleEvents, BattleData>.Subscribe(BattleEvents.StartPlayerStartSequence, (x) => StartCoroutine(OnBattleStart(x)));
        EventManager<BattleEvents, bool>.Subscribe(BattleEvents.SetPlayerInteractable, SetInteractable);
        EventManager<BattleEvents>.Subscribe(BattleEvents.BattleEnd, () => StartCoroutine(OnBattleEnd()));
    }
    private void OnDisable() {
        EventManager<BattleEvents, BattleData>.Unsubscribe(BattleEvents.StartPlayerStartSequence, (x) => StartCoroutine(OnBattleStart(x)));
        EventManager<BattleEvents, bool>.Unsubscribe(BattleEvents.SetPlayerInteractable, SetInteractable);
        EventManager<BattleEvents>.Unsubscribe(BattleEvents.BattleEnd, () => StartCoroutine(OnBattleEnd()));
    }

    public void SetUp(Vector2Int gridPosition) {
        actionQueue = new(() => isWalking = false);
        playerPosition = gridPosition;

        FindAccessibleTiles(playerPosition, 30);
    }

    private void SetInteractable(bool value) {
        if (value)
            interactableStack.Pop();
        else
            interactableStack.Push(value);
    }

    private void Update() {
        if (!IsInteractable)
            return;

        actionQueue?.OnUpdate();

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

        line.enabled = false;
        interactableStack.Push(false);

        yield return new WaitForSeconds(2.5f);
        visuals.SetActive(false);
    }

    private IEnumerator OnBattleEnd() {
        yield return new WaitForSeconds(3f);

        EventManager<CameraEventType, Transform>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_OBJECT, transform);
        EventManager<CameraEventType, float>.Invoke(CameraEventType.CHANGE_CAM_FOLLOW_SPEED, 30);

        visuals.SetActive(true);
        interactableStack.Pop();

        FindAccessibleTiles(playerPosition, 30);
    }

    private void Move() {
        Vector2Int position = MouseToWorldView.HoverTileGridPos;
        if (position == GridStaticFunctions.CONST_EMPTY)
            return;

        EnqueueMovement(position);
        isWalking = true;

        EventManager<UIEvents, string>.Invoke(UIEvents.AddBattleInformation,
            $"<color=green>Player</color> walked to <color=blue>{position}</color>");
    }

    private void EnqueueMovement(Vector2Int targetPosition) {
        actionQueue.Enqueue(new DoMethodAction(() => {
            unitAnimator.SetBool("Walking", true);
        }));

        Vector2Int lastPos = playerPosition;
        foreach (var newPos in GetPath(targetPosition)) {
            Vector2Int lookDirection = GridStaticFunctions.GetVector2RotationFromDirection(GridStaticFunctions.CalcWorldPos(newPos) - GridStaticFunctions.CalcWorldPos(lastPos));

            actionQueue.Enqueue(new ActionStack(
                new MoveObjectAction(gameObject, moveSpeed, GridStaticFunctions.CalcWorldPos(newPos)),
                new RotateAction(visuals, new Vector3(0, GridStaticFunctions.GetRotationFromVector2Direction(lookDirection, true), 0), rotSpeed, .1f)
                ));

            actionQueue.Enqueue(new DoMethodAction(() => {
                this.lookDirection = lookDirection;
                playerPosition = newPos;

                footstepManager.CheckFootstep(footstepManager.LeftFootTransform);
                footstepManager.CheckFootstep(footstepManager.RightFootTransform);
            }));

            lastPos = newPos;
        }

        actionQueue.Enqueue(new DoMethodAction(() => {
            unitAnimator.SetBool("Walking", false);

            FindAccessibleTiles(playerPosition, 30);
            GridStaticFunctions.ResetAllTileColors();
        }));
    }

    private void FindAccessibleTiles(Vector2Int gridPos, int speedValue) {
        parentDictionary.Clear();
        currentAccessableTiles.Clear();

        List<Vector2Int> neighbours = new();
        neighbours.AddRange(GridStaticFunctions.directCubeNeighbours);
        neighbours.AddRange(GridStaticFunctions.diagonalCubeNeighbours);

        openSet.Add(gridPos);
        for (int i = 0; i < speedValue; i++) {
            Queue<Vector2Int> queue = new(openSet);
            openSet.Clear();

            while (queue.Count > 0) {
                Vector2Int currentPos = queue.Dequeue();

                foreach (var offset in neighbours) {
                    Vector2Int neighbour = currentPos + offset;
                    if (closedSet.Contains(neighbour))
                        continue;

                    if (GridStaticFunctions.Grid[neighbour].Type != TileType.Normal)
                        continue;

                    currentAccessableTiles.Add(neighbour);
                    parentDictionary[neighbour] = currentPos;

                    closedSet.Add(neighbour);
                    openSet.Add(neighbour);
                }
            }
        }

        openSet.Clear();
        closedSet.Clear();
    }

    private List<Vector2Int> GetPath(Vector2Int endPos) {
        List<Vector2Int> path = new();
        Vector2Int currentPosition = endPos;

        while (currentPosition != playerPosition) {
            path.Add(currentPosition);

            if (!parentDictionary.ContainsKey(currentPosition))
                return new();

            currentPosition = parentDictionary[currentPosition];
        }

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