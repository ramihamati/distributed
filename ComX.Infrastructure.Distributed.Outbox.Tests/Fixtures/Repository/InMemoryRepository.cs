using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;

public class InMemoryRepository : IOutboxRepository<IntegrationMessageLog>
{
    private List<IntegrationMessageLog> _storage = new();

    public Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        IntegrationMessageLog entityInStore = _storage.Find(r => r.Id == entity.Id);
        if (entityInStore is null)
        {
            return Task.CompletedTask;
        }

        if (!entityInStore.Timestamp.SequenceEqual(entity.Timestamp))
        {
            throw new OutboxConcurrencyException("Could not delete the entity. Timestamp does not match");
        }

        _storage.Remove(entityInStore);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.Any(r => r.Id == entityId));
    }

    public Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        IQueryable<IntegrationMessageLog> query = _storage.AsQueryable();
        query = ApplyFilterOptions(finder, query);
        query = ApplyFinderOptions(finder, query);
        query = query.OrderBy(r => r.CreatedAt);
        return Task.FromResult(query.ToList());
    }

    public Task<IntegrationMessageLog> FindAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.Find(r => r.Id == entityId));
    }

    public Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks);

        if (_storage.Any(r => r.Id == entity.Id))
        {
            throw new Exception($"The id {entity.Id} is already in the store");
        }

        _storage.Add(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> LockAsync(IntegrationMessageLog entity, TimeSpan span)
    {
        try
        {
            entity.LockUntil = DateTime.UtcNow.Add(span);
            await UpdateAsync(entity);
            return true;
        }
        catch (OutboxConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> UnlockAsync(IntegrationMessageLog entity)
    {
        try
        {
            entity.LockUntil = null;
            await UpdateAsync(entity);
            return true;
        }
        catch (OutboxConcurrencyException)
        {
            return false;
        }
    }

    public Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        IntegrationMessageLog entityInStore = _storage.Find(r => r.Id == entity.Id);
        if (entityInStore is null)
        {
            throw new Exception($"Entity with id {entity.Id} is not in store");
        }

        if (!entityInStore.Timestamp.SequenceEqual(entity.Timestamp))
        {
            throw new OutboxConcurrencyException("Could not update the entity. Timestamp does not match");
        }

        _storage.RemoveAll(r => r.Id == entity.Id);
        entity.Timestamp = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        _storage.Add(entity);
        return Task.CompletedTask;
    }

    #region [ Private ]
    private static IQueryable<IntegrationMessageLog> ApplyFilterOptions(FinderMessageLog finder, IQueryable<IntegrationMessageLog> query)
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

    private static IQueryable<IntegrationMessageLog> ApplyFinderOptions(FinderMessageLog finder, IQueryable<IntegrationMessageLog> query)
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
