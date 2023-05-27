public class DialogFunctionality
{
    public DialogSystem Owner;

    public void SetEvents() {
        EventManager<DialogEvents, bool>.Subscribe(DialogEvents.ON_TEST_EVENT, OnTestCallRun);
        EventManager<DialogEvents, float>.Subscribe(DialogEvents.SET_TYPE_TIME, SetSpeed);
    }

    public void OnTestCallRun(bool yes) {

    }

    public void SetSpeed(float speed) {
        Owner.CurrentTimeBetweenChars = speed;
    }
}

public enum DialogEvents {
    ON_TEST_EVENT,
    SET_TYPE_TIME,
}