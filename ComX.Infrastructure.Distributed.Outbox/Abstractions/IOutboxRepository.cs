namespace ComX.Infrastructure.Distributed.Outbox;

public interface IOutboxRepository
{
    Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task<IntegrationMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default);
    Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog findOptions,
        CancellationToken cancellationToken = default);
}
