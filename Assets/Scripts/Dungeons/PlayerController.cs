using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float moveSpeed = 10;

    private ActionQueue actionQueue;

    void Start() {
        actionQueue = new();
    }

    void Update() {
        actionQueue.OnUpdate();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Move();
    }

    private void Move() {
        Vector2Int position = MouseToWorldView.HoverTileGridPos;
        if (position == GridStaticFunctions.CONST_EMPTY)
            return;

        Vector3 calculatedMovePosition = GridStaticFunctions.Grid[position].StandardWorldPosition;

        actionQueue.Clear();
        actionQueue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, calculatedMovePosition));
    }
}