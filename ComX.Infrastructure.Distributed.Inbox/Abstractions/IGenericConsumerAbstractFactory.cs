namespace ComX.Infrastructure.Distributed.Inbox;

public interface IGenericConsumerAbstractFactory
{
    IGenericConsumerFactory<TEvent> Create<TEvent>() where TEvent : class;
}
