namespace ComX.Infrastructure.Distributed.Inbox;

public class EventTypeRegistry : IEventTypeRegistry
{
    private List<EventTypeInfo> EventTypes { get; }

    public EventTypeRegistry(List<EventTypeInfo> eventTypeInfos)
    {
        EventTypes = eventTypeInfos;
    }

    public EventTypeInfo? GetInfoFor<TEvent>()
    {
        var type = typeof(TEvent);
        return EventTypes.Find(r => r.EventType == type);
    }

    public EventTypeInfo? GetInfoFor(string name)
    {
        return EventTypes.Find(r => string.Equals(r.Name, name, StringComparison.InvariantCultureIgnoreCase));
    }

    public IEnumerable<EventTypeInfo> GetEventTypes()
    {
        foreach(var type in EventTypes)
        {
            yield return type;
        }
    }
}
