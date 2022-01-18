namespace ComX.Infrastructure.Distributed.Outbox
{
    public interface IOutboxStorage
    {
        Task InsertAsync(IntegrationMessageLog item, CancellationToken cancellationToken = default);
        Task UpdateAsync(IntegrationMessageLog item, CancellationToken cancellationToken = default);
        Task DeleteAsync(IntegrationMessageLog item, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IntegrationMessageLog> FindAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<IntegrationMessageLog>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default);
        Task<bool> LockAsync(IntegrationMessageLog entity, TimeSpan span);
        Task<bool> UnlockAsync(IntegrationMessageLog entity);
    }
}
