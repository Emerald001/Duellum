using UnityEngine.EventSystems;

public class PlayerCardBehaviour : BaseCardBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler {

    public void OnPointerEnter(PointerEventData eventData) {
        if (grabbed || !CanInvoke)
            return;

        OnHoverEnter.Invoke(this, () => {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Hover);

            queue.Clear();
            resizeQueue.Clear();
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, raisedPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, raisedSize));
        });
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (grabbed || !CanInvoke)
            return;

        OnHoverExit.Invoke(this, () => {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);

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

        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Grab);
        EventManager<BattleEvents>.Invoke(BattleEvents.GrabbedAbilityCard);

        queue.Clear();
        resizeQueue.Clear();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (!CanInvoke)
            return;

        grabbed = false;
        OnMoveRelease.Invoke(this, () => {
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, standardPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));
        });

        EventManager<BattleEvents>.Invoke(BattleEvents.ReleasedAbilityCard);
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
    }

    public void OnPointerMove(PointerEventData eventData) {
        if (!grabbed || !CanInvoke)
            return;
    }
}
