using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCardBehaviour : MonoBehaviour {
    public static System.Action<BaseCardBehaviour, System.Action> OnHoverEnter;
    public static System.Action<BaseCardBehaviour, System.Action> OnHoverExit;
    public static System.Action<BaseCardBehaviour> OnClick;

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
    protected Vector3 selectedPos;

    protected Vector3 standardSize;
    protected Vector3 raisedSize;

    protected Vector3 offset;

    protected static bool selected = false;

    private void Awake() {
        standardSize = transform.localScale;
        raisedSize = standardSize * scaleModifier;
    }

    public void SetValues(Vector3 raisedPos, Vector3 selectedPos, Camera UICam, int index) {
        standardPos = transform.position;

        this.raisedPos = raisedPos;
        this.selectedPos = selectedPos;

        this.UICam = UICam;
        Index = index;

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

    public void ClearQueue(bool finishAction = false) {
        queue.Clear(finishAction);
    }
}