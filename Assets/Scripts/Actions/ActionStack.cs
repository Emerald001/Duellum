using System.Collections.Generic;

public class ActionStack : Action
{
    public ActionStack(params Action[] actionsToPerform) {
        this.actionsToPerform = new(actionsToPerform);
    }

    private readonly List<Action> actionsToPerform;

    public override void OnEnter() { }
    public override void OnExit() { }

    public override void OnUpdate() {
        for (int i = actionsToPerform.Count - 1; i >= 0; i--) {
            Action action = actionsToPerform[i];
            action.OnUpdate();

            if (action.IsDone)
                actionsToPerform.Remove(action);
        }

        if (actionsToPerform.Count < 1)
            IsDone = true;
    }
}
