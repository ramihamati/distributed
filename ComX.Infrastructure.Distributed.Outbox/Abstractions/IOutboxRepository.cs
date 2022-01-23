namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxRepository<TMessage>
    where TMessage : class, IIntegrationMessageLog
{
    Task DeleteAsync(TMessage entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task<TMessage> FindAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task InsertAsync(TMessage entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TMessage entity, CancellationToken cancellationToken = default);
    Task<List<TMessage>> FindAsync(
        FinderMessageLog findOptions,
        CancellationToken cancellationToken = default);
}
