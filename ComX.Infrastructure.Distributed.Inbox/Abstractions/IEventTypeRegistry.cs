namespace ComX.Infrastructure.Distributed.Inbox;

public interface IEventTypeRegistry
{
    EventTypeInfo? GetInfoFor<TEvent>();
    EventTypeInfo? GetInfoFor(string name);
    IEnumerable<EventTypeInfo> GetEventTypes();
}