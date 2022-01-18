namespace ComX.Infrastructure.Distributed.Inbox;

public interface IInboxBusConfigurator
{
    void RegisterConsumer<TEvent>() where TEvent : class;
}
