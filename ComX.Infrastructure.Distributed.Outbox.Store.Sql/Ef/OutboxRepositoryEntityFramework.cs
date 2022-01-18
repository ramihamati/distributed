using System.Data;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Wrapper over URF repository because we want the flexibility of 
/// replacing the repository only with the required method
/// 
/// And because in some scenarios we must use an external repository we expose only some methods
/// </summary>
public class OutboxRepositoryEntityFramework : IOutboxRepository
{
    private readonly IRepository<IntegrationMessageLog> _repository;

    public OutboxRepositoryEntityFramework(
        IRepository<IntegrationMessageLog> repository)
    {
        _repository = repository;
    }
    public Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        _repository.Delete(entity);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return _repository.ExistsAsync(entityId, cancellationToken);
    }

    public Task<IntegrationMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(entityId, cancellationToken);
    }

    public async Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        IQuery<IntegrationMessageLog> query = _repository.Query();
        
        query = ApplyFilterOptions(finder, query);
        query = ApplyFinderOptions(finder, query);
        query = query.OrderBy(r => r.CreatedAt);

        return (await query.SelectAsync(cancellationToken)).ToList();
    }

    public Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        _repository.Insert(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        _repository.Update(entity);
        return Task.CompletedTask;
    }

    #region [ Private ]
    private static IQuery<IntegrationMessageLog> ApplyFilterOptions(FinderMessageLog finder, IQuery<IntegrationMessageLog> query)
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

    private static IQuery<IntegrationMessageLog> ApplyFinderOptions(FinderMessageLog finder, IQuery<IntegrationMessageLog> query)
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
