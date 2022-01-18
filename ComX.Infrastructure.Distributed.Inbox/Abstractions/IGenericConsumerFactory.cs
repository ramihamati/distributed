namespace ComX.Infrastructure.Distributed.Inbox;

public interface IGenericConsumerFactory<TEvent> where TEvent : class
{
    IGenericConsumer<TEvent> Create();
}
