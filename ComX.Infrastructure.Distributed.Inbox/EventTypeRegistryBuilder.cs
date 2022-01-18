namespace ComX.Infrastructure.Distributed.Inbox;

public class EventTypeRegistryBuilder : IEventTypeRegistryBuilder
{
    private bool _sealed = false;
    private List<EventTypeInfo> EventTypes { get; }

    public EventTypeRegistryBuilder()
    {
        EventTypes = new List<EventTypeInfo>();
    }

    public void RegisterEventType<TEventType>(string name)
    {
        EnsureNotSealed();

        if (EventTypes.Any(r => string.Equals(r.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception("An event with the same name is already registered");
        }

        EventTypes.Add(new EventTypeInfo(typeof(TEventType), name));
    }

    private void Seal()
    {
        _sealed = true;
    }

    private void EnsureNotSealed()
    {
        if (_sealed)
        {
            throw new Exception("The registry is sealed. Cannot perform register operations");
        }
    }

    public IEventTypeRegistry Build()
    {
        Seal();
        return new EventTypeRegistry(EventTypes);
    }
}
