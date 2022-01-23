using System.Data;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxStorageEntityFramework<TMessageLog> : IOutboxStorage<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    private readonly IOutboxRepository<TMessageLog> _repository;
    /// <summary>
    /// In the case where we want to commit transactions alongside other transactions on the project where
    /// the system is added, the unit of work here must be a Noop one
    /// </summary>
    private readonly IOutboxUnitOfWork _unitOfWork;

    public OutboxStorageEntityFramework(
        IOutboxRepository<TMessageLog> repository,
        IOutboxUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteAsync(TMessageLog item, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public Task<TMessageLog> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(id, cancellationToken);
    }

    public async Task InsertAsync(TMessageLog item, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TMessageLog item, CancellationToken cancellationToken = default)
    {
        await _repository.UpdateAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<List<TMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(finder, cancellationToken);
    }

    public async Task<bool> LockAsync(TMessageLog entity, TimeSpan span)
    {
        try
        {
            entity.LockUntil = DateTime.UtcNow.Add(span);
            await _repository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (DBConcurrencyException)
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
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (DBConcurrencyException)
        {
            return false;
        }
    }
}
