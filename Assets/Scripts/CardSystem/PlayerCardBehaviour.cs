using UnityEngine.EventSystems;

public class PlayerCardBehaviour : BaseCardBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public void OnPointerEnter(PointerEventData eventData) {
        if (selected || !CanInvoke)
            return;

        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_shuffleCards");
        OnHoverEnter.Invoke(this, () => {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Hover);

            queue.Clear();
            resizeQueue.Clear();
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, raisedPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, raisedSize));
        });
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (selected || !CanInvoke)
            return;

        OnHoverExit.Invoke(this, () => {
            EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);

            queue.Clear();
            resizeQueue.Clear();
            queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, standardPos));
            resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));
        });
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (selected || !CanInvoke)
            return;

        selected = true;
        OnClick.Invoke(this);

        CardHandStateMachine.OnDismiss += DeselectCard;
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_grabCard");
    }

    public void DeselectCard() {
        selected = false;

        queue.Enqueue(new MoveObjectAction(gameObject, moveSpeed, selectedPos));
        resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));

        CardHandStateMachine.OnDismiss -= DeselectCard;
    }
}
