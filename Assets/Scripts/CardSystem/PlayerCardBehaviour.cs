﻿using UnityEngine.EventSystems;

public class PlayerCardBehaviour : BaseCardBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
    public void OnPointerEnter(PointerEventData eventData) {
        if (grabbed || !CanInvoke)
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
        EventManager<UIEvents>.Invoke(UIEvents.GrabbedAbilityCard);
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_grabCard");

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

        EventManager<UIEvents>.Invoke(UIEvents.ReleasedAbilityCard);
        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
    }
}
