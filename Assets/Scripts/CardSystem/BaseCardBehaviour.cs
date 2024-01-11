using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCardBehaviour : MonoBehaviour {
    public static System.Action<BaseCardBehaviour, System.Action> OnHoverEnter;
    public static System.Action<BaseCardBehaviour, System.Action> OnHoverExit;
    public static System.Action<BaseCardBehaviour, System.Action> OnMoveRelease;
    public static System.Action<BaseCardBehaviour> OnMove;

    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float resizeSpeed;
    [SerializeField] protected float scaleModifier;

    protected readonly ActionQueue queue = new();
    protected readonly ActionQueue resizeQueue = new();

    public Vector3 StandardPosition => standardPos;
    public bool CanInvoke { get; set; }
    public int Index { get; set; }

    protected Camera UICam;

    protected Vector3 standardPos;
    protected Vector3 raisedPos;

    protected Vector3 standardSize;
    protected Vector3 raisedSize;

    protected Vector3 offset;

    protected bool grabbed = false;

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

    public void ClearQueue(bool finishAction = false) {
        queue.Clear(finishAction);
    }
}