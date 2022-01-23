using System.Data;

namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Storage when using an external custom IOuboxRepository.
/// No requirements regarding the persistance layer are required, user handles providing an implementation for
/// the <see cref="IOutboxRepository{TMessage}"/>.
/// 
/// Note that the worker requires a repository that will perform actual changes on update. E.G. Unit Of Work being called
/// on each RUD methods in <see cref="IOutboxRepository{TMessage}"/>
/// </summary>
public class OutboxStorageRepository<TMessage> : IOutboxStorage<TMessage>
    where TMessage : class, IIntegrationMessageLog
{
    private readonly IOutboxRepository<TMessage> _repository;

    public OutboxStorageRepository(
        IOutboxRepository<TMessage> repository)
    {
        _repository = repository;
    }

    public async Task DeleteAsync(TMessage item, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(item, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public Task<TMessage> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(id, cancellationToken);
    }

    public async Task InsertAsync(TMessage item, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(item, cancellationToken);
    }

    public async Task UpdateAsync(TMessage item, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(item, cancellationToken);
    }

    public Task<List<TMessage>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(finder, cancellationToken);
    }

    public async Task<bool> LockAsync(TMessage entity, TimeSpan span)
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

    public async Task<bool> UnlockAsync(TMessage entity)
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
