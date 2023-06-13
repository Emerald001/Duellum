using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public static event System.Action<CardBehaviour, System.Action> OnHoverEnter;
    public static event System.Action<CardBehaviour, System.Action> OnHoverExit;

    [SerializeField] float moveSpeed;
    [SerializeField] float resizeSpeed;
    [SerializeField] float scaleModifier;

    private readonly ActionQueue queue = new();

    public Vector3 StandardPosition => standardPos;
    private Vector3 standardPos;
    private Vector3 raisedPos;

    private Vector3 standardSize;
    private Vector3 raisedSize;

    public bool canInvoke { get; set; }

    public void SetValues(Vector3 raisedPos) {
        standardPos = transform.position;
        standardSize = transform.localScale;

        this.raisedPos = raisedPos;
        raisedSize = standardSize * scaleModifier;

        canInvoke = true;
    }

    private void Update() {
        queue.OnUpdate();
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue(bool finishAction = false) => queue.Clear(finishAction);

    public void OnPointerEnter(PointerEventData eventData) {
        if (!canInvoke)
            return;

        OnHoverEnter.Invoke(this, () =>
        {
            queue.Clear();
            queue.Enqueue(new ActionStack(
                new MoveObjectAction(gameObject, moveSpeed, raisedPos),
                new ResizeAction(transform, resizeSpeed, raisedSize)));
        });
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!canInvoke)
            return;

        OnHoverExit.Invoke(this, () =>
        {
            queue.Clear();
            queue.Enqueue(new ActionStack(
                new MoveObjectAction(gameObject, moveSpeed, standardPos),
                new ResizeAction(transform, resizeSpeed, standardSize)));
        });
    }
}
