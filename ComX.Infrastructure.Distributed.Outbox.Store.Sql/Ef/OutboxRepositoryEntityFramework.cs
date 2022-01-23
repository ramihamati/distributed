using System.Data;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Wrapper over URF repository because we want the flexibility of 
/// replacing the repository only with the required method
/// 
/// And because in some scenarios we must use an external repository we expose only some methods
/// </summary>
public class OutboxRepositoryEntityFramework<TMessageLog> : IOutboxRepository<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    private readonly IRepository<TMessageLog> _repository;

    public OutboxRepositoryEntityFramework(
        IRepository<TMessageLog> repository)
    {
        _repository = repository;
    }
    public Task DeleteAsync(TMessageLog entity, CancellationToken cancellationToken = default)
    {
        _repository.Delete(entity);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(entityId, cancellationToken);
    }

    public Task<TMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(entityId, cancellationToken);
    }

    public async Task<List<TMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        IQuery<TMessageLog> query = _repository.Query();
        
        query = ApplyFilterOptions(finder, query);
        query = ApplyFinderOptions(finder, query);
        query = query.OrderBy(r => r.CreatedAt);

        return (await query.SelectAsync(cancellationToken)).ToList();
    }

    public Task InsertAsync(TMessageLog entity, CancellationToken cancellationToken = default)
    {
        _repository.Insert(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TMessageLog entity, CancellationToken cancellationToken = default)
    {
        _repository.Update(entity);
        return Task.CompletedTask;
    }

    #region [ Private ]
    private static IQuery<TMessageLog> ApplyFilterOptions(FinderMessageLog finder, IQuery<TMessageLog> query)
    {
        FilterMessageLog filter = finder.Filter;

        if (filter.LastAttemptOffset.HasValue)
        {
            DateTime nextAttempt = DateTime.UtcNow - filter.LastAttemptOffset.Value;
            query = query.Where(r => r.LastAttemptDate == null || r.LastAttemptDate < nextAttempt);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(r => r.Status == filter.Status.Value);
        }

        if (filter.Unlocked.HasValue)
        {
            query = query.Where(r => r.LockUntil == null || r.LockUntil < DateTime.UtcNow);
        }

        if (filter.MessageTypeName.HasValue)
        {
            query = query.Where(r => r.MessageTypeName == filter.MessageTypeName.Value);
        }

        return query;
    }

    private static IQuery<TMessageLog> ApplyFinderOptions(FinderMessageLog finder, IQuery<TMessageLog> query)
    {
        if (finder.Skip.HasValue)
        {
            query = query.Skip(finder.Skip.Value);
        }

        if (finder.Limit.HasValue)
        {
            query = query.Take(finder.Limit.Value);
        }

        return query;
    }

    #endregion
}
