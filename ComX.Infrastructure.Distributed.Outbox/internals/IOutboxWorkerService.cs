namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Services that publishes an IntegrationMessageLog to a broker.
/// It reads a payload and deserialize the event to the underlying event type
/// as registered <see cref="RegistryMessageInfo.MessageType"/> using the <see cref="IntegrationMessageLog.MessageTypeName"/>
/// as the discriminator
/// </summary>
public interface IOutboxWorkerService
{
    Task<List<IntegrationMessageLog>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default);
    Task PublishAsync(IntegrationMessageLog message, CancellationToken cancellationToken = default);
}
