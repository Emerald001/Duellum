public struct EventMessage<T> {
    public T value;

    public EventMessage(T value) {
        this.value = value;
    }
}

public struct EventMessage<T1, T2> {
    public T1 value1;
    public T2 value2;

    public EventMessage(T1 value1, T2 value2) {
        this.value1 = value1;
        this.value2 = value2;
    }
}

public struct EventMessage<T1, T2, T3> {
    public T1 value1;
    public T2 value2;
    public T3 value3;

    public EventMessage(T1 value1, T2 value2, T3 value3) {
        this.value1 = value1;
        this.value2 = value2;
        this.value3 = value3;
    }
}

public struct EventMessage<T1, T2, T3, T4> {
    public T1 value1;
    public T2 value2;
    public T3 value3;
    public T4 value4;

    public EventMessage(T1 value1, T2 value2, T3 value3, T4 value4) {
        this.value1 = value1;
        this.value2 = value2;
        this.value3 = value3;
        this.value4 = value4;
    }
}