namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Services that publishes an IntegrationMessageLog to a broker.
/// It reads a payload and deserialize the event to the underlying event type
/// as registered <see cref="RegistryMessageInfo.MessageType"/> using the <see cref="IntegrationMessageLog.MessageTypeName"/>
/// as the discriminator
/// </summary>
public interface IOutboxWorkerService<TMessageLog>
        where TMessageLog : class, IIntegrationMessageLog
{
    Task<List<TMessageLog>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default);
    Task PublishAsync(TMessageLog message, CancellationToken cancellationToken = default);
}
