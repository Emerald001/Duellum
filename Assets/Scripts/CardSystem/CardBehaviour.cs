using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardBehaviour : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler {
    public static event System.Action<CardBehaviour, System.Action> OnHoverEnter;
    public static event System.Action<CardBehaviour, System.Action> OnHoverExit;
    public static event System.Action<CardBehaviour, System.Action> OnMoveRelease;
    public static event System.Action<CardBehaviour> OnMove;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float resizeSpeed;
    [SerializeField] private float scaleModifier;

    private readonly ActionQueue queue = new();
    private readonly ActionQueue resizeQueue = new();

    public Vector3 StandardPosition => standardPos;
    public bool CanInvoke { get; set; }
    public int Index { get; set; }

    private Camera UICam;

    private Vector3 standardPos;
    private Vector3 raisedPos;

    private Vector3 standardSize;
    private Vector3 raisedSize;

    private Vector3 offset;

    private bool grabbed = false;

    public void SetValues(Vector3 raisedPos, Camera UICam, int index) {
        standardPos = transform.position;
        standardSize = transform.localScale;

        this.raisedPos = raisedPos;
        raisedSize = standardSize * scaleModifier;

        this.UICam = UICam;
        Index = index;

        CanInvoke = true;
    }

    private void Update() {
        if (grabbed && CanInvoke) {
            transform.position = UICam.ScreenToWorldPoint(Input.mousePosition) + offset;
            OnMove.Invoke(this);
        }

        queue.OnUpdate();
        resizeQueue.OnUpdate();
    }

    public void SetActionQueue(List<Action> actions) {
        foreach (var item in actions)
            queue.Enqueue(item);
    }

    public void ClearQueue(bool finishAction = false) => queue.Clear(finishAction);

    public void OnPointerEnter(PointerEventData eventData) {
        if (grabbed || !CanInvoke)
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
        if (grabbed || !CanInvoke)
            return;

        OnHoverExit.Invoke(this, () =>
        {
            queue.Clear();
            resizeQueue.Clear();
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, standardPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));
        });
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!CanInvoke)
            return;

        grabbed = true; 
        offset = transform.position - UICam.ScreenToWorldPoint(eventData.position);

        queue.Clear();
        resizeQueue.Clear();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!CanInvoke)
            return;

        grabbed = false;

        OnMoveRelease.Invoke(this, () =>
        {
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, standardPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));
        });
    }

    public void OnPointerMove(PointerEventData eventData) {
        if (!grabbed || !CanInvoke)
            return;
    }
}
