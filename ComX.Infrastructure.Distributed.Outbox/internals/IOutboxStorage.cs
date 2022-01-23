namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxStorage<TMessage>
    where TMessage : class, IIntegrationMessageLog
{
    Task InsertAsync(TMessage item, CancellationToken cancellationToken = default);
    Task UpdateAsync(TMessage item, CancellationToken cancellationToken = default);
    Task DeleteAsync(TMessage item, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TMessage> FindAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TMessage>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default);
    Task<bool> LockAsync(TMessage entity, TimeSpan span);
    Task<bool> UnlockAsync(TMessage entity);
}
