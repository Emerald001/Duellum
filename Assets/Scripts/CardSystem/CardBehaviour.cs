using UnityEngine;
using UnityEngine.EventSystems;

public class CardBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private Transform standardPos;
    [SerializeField] private Transform raisedPos;

    [SerializeField] float moveSpeed;
    [SerializeField] float resizeSpeed;

    private readonly ActionQueue queue = new();

    private Vector3 originalSize;

    private void Awake() {
        originalSize = transform.localScale;
    }

    private void Update() {
        queue.OnUpdate();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        queue.Clear();
        queue.Enqueue(new ActionStack(
            new MoveObjectAction(gameObject, moveSpeed, raisedPos),
            new ResizeAction(transform, resizeSpeed, new Vector3(1, 1, 1))));
    }

    public void OnPointerExit(PointerEventData eventData) {
        queue.Clear();
        queue.Enqueue(new ActionStack(
            new MoveObjectAction(gameObject, moveSpeed, standardPos),
            new ResizeAction(transform, resizeSpeed, originalSize)));
    }
}
