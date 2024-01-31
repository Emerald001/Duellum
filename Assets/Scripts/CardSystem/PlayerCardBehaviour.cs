using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class PlayerCardBehaviour : BaseCardBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public void OnPointerEnter(PointerEventData eventData) {
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, false);
        
        if (selected || !CanInvoke) 
            return;

        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ui_Click");
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
        EventManager<BattleEvents, bool>.Invoke(BattleEvents.SetPlayerInteractable, true);

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
        CardHandStateMachine.OnDismiss += DeselectCard;

        queue.Enqueue(new ActionStack(
            new MoveObjectAction(gameObject, moveSpeed, selectedPos),
            new RotateAction(gameObject, Vector3.zero, resizeSpeed, .1f),
            new ResizeAction(transform, resizeSpeed, standardSize)));

        EventManager<UIEvents, CursorType>.Invoke(UIEvents.UpdateCursor, CursorType.Normal);
        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_grabCard");

        OnClick.Invoke(this);

        if (!CanInvoke) UnableToUseCard();
    }


    public void DeselectCard() {
        selected = false;

        resizeQueue.Enqueue(new ResizeAction(transform, resizeSpeed, standardSize));

        CardHandStateMachine.OnDismiss -= DeselectCard;
    }

    private void UnableToUseCard() {
        EventManager<UIEvents, string>.Invoke(UIEvents.InfoTextUpdate, "Target is not in range!");
    }
}
