using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public static event System.Action<CardBehaviour, System.Action> OnHoverEnter;
    public static event System.Action<CardBehaviour, System.Action> OnHoverExit;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float resizeSpeed;
    [SerializeField] private float scaleModifier;

    private readonly ActionQueue queue = new();
    private readonly ActionQueue resizeQueue = new();

    public Vector3 StandardPosition => standardPos;
    public bool CanInvoke { get; set; }

    private Vector3 standardPos;
    private Vector3 raisedPos;

    private Vector3 standardSize;
    private Vector3 raisedSize;

    public void SetValues(Vector3 raisedPos) {
        standardPos = transform.position;
        standardSize = transform.localScale;

        this.raisedPos = raisedPos;
        raisedSize = standardSize * scaleModifier;

        CanInvoke = true;
    }

    private void Update() {
        queue.OnUpdate();
        resizeQueue.OnUpdate();
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue(bool finishAction = false) => queue.Clear(finishAction);

    public void OnPointerEnter(PointerEventData eventData) {
        if (!CanInvoke)
            return;

        OnHoverEnter.Invoke(this, () =>
        {
            queue.Clear();
            resizeQueue.Clear();
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, raisedPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, raisedSize));
        });
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!CanInvoke)
            return;

        OnHoverExit.Invoke(this, () =>
        {
            queue.Clear();
            resizeQueue.Clear();
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, standardPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));
        });
    }
}
