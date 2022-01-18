namespace ComX.Infrastructure.Distributed.Inbox;

public class EventTypeInfo
{
    public Type EventType { get; }

    public string Name { get; }

    public EventTypeInfo(
        Type eventType,
        string name)
    {
        EventType = eventType;
        Name = name;
    }
}
