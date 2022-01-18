namespace ComX.Infrastructure.Distributed.Inbox;

public interface IEventTypeRegistryBuilder
{
    void RegisterEventType<TEventType>(string name);

    IEventTypeRegistry Build();
}
