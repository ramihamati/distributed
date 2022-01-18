namespace ComX.Infrastructure.Distributed.Inbox;

/// <summary>
/// The broker consumer will forward processing of the message to this consumer
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IGenericConsumer<TEvent> where TEvent : class
{
    Task Consume(TEvent message);
}
