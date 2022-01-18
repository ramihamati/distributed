using System.Data;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxStorageRepository : IOutboxStorage
{
    private readonly IOutboxRepository _repository;

    public OutboxStorageRepository(
        IOutboxRepository repository)
    {
        _repository = repository;
    }

    public async Task DeleteAsync(IntegrationMessageLog item, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(item, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public Task<IntegrationMessageLog> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(id, cancellationToken);
    }

    public async Task InsertAsync(IntegrationMessageLog item, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(item, cancellationToken);
    }

    public async Task UpdateAsync(IntegrationMessageLog item, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(item, cancellationToken);
    }

    public Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(finder, cancellationToken);
    }

    public async Task<bool> LockAsync(IntegrationMessageLog entity, TimeSpan span)
    {
        try
        {
            entity.LockUntil = DateTime.UtcNow.Add(span);
            await _repository.UpdateAsync(entity);
            return true;
        }
        catch (DBConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> UnlockAsync(IntegrationMessageLog entity)
    {
        try
        {
            entity.LockUntil = DateTime.MinValue;
            await _repository.UpdateAsync(entity);
            return true;
        }
        catch (DBConcurrencyException)
        {
            return false;
        }
    }
}
