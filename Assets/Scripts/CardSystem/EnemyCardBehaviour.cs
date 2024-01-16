using UnityEngine;

public class EnemyCardBehaviour : BaseCardBehaviour {
    public void SelectCard() {
        if (selected || !CanInvoke)
            return;

        queue.Enqueue(new ActionStack(
            new MoveObjectAction(gameObject, moveSpeed, selectedPos),
            new RotateAction(gameObject, Vector3.zero, resizeSpeed, .1f),
            new ResizeAction(transform, resizeSpeed, standardSize)));

        EventManager<AudioEvents, string>.Invoke(AudioEvents.PlayAudio, "ph_grabCard");
    }
}