namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMongoStorage<TMessageLog> : IOutboxStorage<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    private readonly IOutboxRepository<TMessageLog> _repository;

    public OutboxMongoStorage(IOutboxRepository<TMessageLog> outboxRepository)
    {
        _repository = outboxRepository;
    }

    public Task DeleteAsync(TMessageLog item, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(item, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public Task<TMessageLog> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(id, cancellationToken);
    }

    public Task<List<TMessageLog>> FindAsync(FinderMessageLog finder, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(finder, cancellationToken);
    }

    public Task InsertAsync(TMessageLog item, CancellationToken cancellationToken = default)
    {
        return _repository.InsertAsync(item, cancellationToken);
    }

    public async Task<bool> LockAsync(TMessageLog entity, TimeSpan span)
    {
        try
        {
            entity.LockUntil = DateTime.UtcNow.Add(span);
            await _repository.UpdateAsync(entity);
            return true;
        }
        catch (OutboxConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> UnlockAsync(TMessageLog entity)
    {
        try
        {
            entity.LockUntil = DateTime.MinValue;
            await _repository.UpdateAsync(entity);
            return true;
        }
        catch (OutboxConcurrencyException)
        {
            return false;
        }
    }

    public Task UpdateAsync(TMessageLog item, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(item, cancellationToken);
    }
}
