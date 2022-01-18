using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;

public class MongoExternalRepository : IOutboxRepository
{
    private readonly IMongoCollection<MongoOutboxDocument> _collection;

    public MongoExternalRepository(
        OutboxMongoManager manager,
        IOutboxMongoSettings mongoSettings)
    {
        _collection = manager.GetCollection<MongoOutboxDocument>(
            mongoSettings.DbName, mongoSettings.CollectionName);
    }

    public async Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        MongoOutboxDocument mongoEntity = MessageConverter.ToMongoIntegrationMessageLog(entity);
        FilterDefinition<MongoOutboxDocument> filter =
                     Builders<MongoOutboxDocument>.Filter.Eq(t => t.Id, mongoEntity.Id)
                   & Builders<MongoOutboxDocument>.Filter.Eq(t => t.Timestamp, mongoEntity.Timestamp);

        DeleteResult result = await _collection.DeleteOneAsync(filter, cancellationToken: cancellationToken);

        if (!result.IsAcknowledged)
        {
            throw new OutboxException("The mongo delete operation was not acknowledged. " +
                "Please configure you're collection to use WriteConcern=\'Acknowledged\'");
        }

        if (result.DeletedCount != 1)
        {
            if (_collection.CountDocuments(r => r.Id == mongoEntity.Id, cancellationToken: cancellationToken) == 1)
            {
                throw new OutboxConcurrencyException($"Could not update entity \'{mongoEntity.Id}\' because it failed timestamp check");
            }

            throw new OutboxException($"Could not update entity \'{mongoEntity.Id}\'");
        }
    }

    public async Task<bool> ExistsAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        IAsyncCursor<MongoOutboxDocument> entity = await _collection.FindAsync(
            Builders<MongoOutboxDocument>.Filter.Eq(r => r.Id, entityId),
            new FindOptions<MongoOutboxDocument>
            {
                Limit = 1
            }, cancellationToken);

        return entity
            .ToList(cancellationToken: cancellationToken)
            .Any();
    }

    public async Task<IntegrationMessageLog> FindAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        IAsyncCursor<MongoOutboxDocument> entityResponse = await _collection.FindAsync(
            Builders<MongoOutboxDocument>.Filter.Eq(r => r.Id, entityId),
            new FindOptions<MongoOutboxDocument>
            {
                Limit = 1
            }, cancellationToken);

        var entity = entityResponse
            .ToList(cancellationToken: cancellationToken)
            .FirstOrDefault();

        return entity is null ? null : MessageConverter.ToIntegrationMessageLog(entity);
    }

    public async Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<MongoOutboxDocument> filter = GetFilterDefinition(finder);
        FindOptions<MongoOutboxDocument> findOptions = GetFindOptions(finder);

        IAsyncCursor<MongoOutboxDocument> entities = await _collection.FindAsync(filter, findOptions);
        return (await entities.ToListAsync(cancellationToken)).ConvertAll(r => MessageConverter.ToIntegrationMessageLog(r));
    }

    public async Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        MongoOutboxDocument mongoEntity = MessageConverter.ToMongoIntegrationMessageLog(entity);
        mongoEntity.Id = mongoEntity.Id == Guid.Empty ? Guid.NewGuid() : mongoEntity.Id;
        mongoEntity.Timestamp = DateTime.UtcNow.Ticks;
        await _collection.InsertOneAsync(mongoEntity, cancellationToken: cancellationToken);

        // if passed argument is used by reference in other places
        entity.Timestamp = BitConverter.GetBytes(mongoEntity.Timestamp);
        entity.Id = mongoEntity.Id;
    }

    public async Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        MongoOutboxDocument mongoEntity = MessageConverter.ToMongoIntegrationMessageLog(entity);
        long currentTimestamp = mongoEntity.Timestamp;
        mongoEntity.Timestamp = DateTimeOffset.UtcNow.Ticks;

        FilterDefinition<MongoOutboxDocument> filter =
                     Builders<MongoOutboxDocument>.Filter.Eq(t => t.Id, mongoEntity.Id)
                   & Builders<MongoOutboxDocument>.Filter.Eq(t => t.Timestamp, currentTimestamp);

        ReplaceOneResult result = await _collection.ReplaceOneAsync(filter, mongoEntity, cancellationToken: cancellationToken);

        if (!result.IsAcknowledged)
        {
            throw new OutboxException("The mongo update operation was not acknowledged. " +
                "Please configure you're collection to use WriteConcern=\'Acknowledged\'");
        }

        if (result.ModifiedCount != 1)
        {
            if (_collection.CountDocuments(r => r.Id == mongoEntity.Id, cancellationToken: cancellationToken) == 1)
            {
                throw new OutboxConcurrencyException($"Could not update entity \'{mongoEntity.Id}\' because it failed timestamp check");
            }

            throw new OutboxException($"Could not update entity \'{mongoEntity.Id}\'");
        }

        // if passed argument is used by reference in other places
        entity.Timestamp = BitConverter.GetBytes(mongoEntity.Timestamp);
    }

    public async Task<bool> UnlockAsync(IntegrationMessageLog entity)
    {
        try
        {
            entity.LockUntil = DateTime.MinValue;
            await UpdateAsync(entity);
            return true;
        }
        catch (OutboxConcurrencyException)
        {
            return false;
        }
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

    #region [ Private ]

    private static FindOptions<MongoOutboxDocument> GetFindOptions(FinderMessageLog finder)
    {
        FindOptions<MongoOutboxDocument> findOptions = new();

        if (finder.Limit.HasValue)
        {
            findOptions.Limit = finder.Limit.Value;
        }

        if (finder.Skip.HasValue)
        {
            findOptions.Skip = finder.Skip.Value;
        }

        findOptions.Sort = Builders<MongoOutboxDocument>.Sort.Ascending(r => r.CreatedAt);

        return findOptions;
    }

    private static FilterDefinition<MongoOutboxDocument> GetFilterDefinition(FinderMessageLog findOptions)
    {
        FilterDefinition<MongoOutboxDocument> filter = FilterDefinition<MongoOutboxDocument>.Empty;

        if (findOptions.Filter.LastAttemptOffset.HasValue)
        {
            DateTime nextAttempt = DateTime.UtcNow - findOptions.Filter.LastAttemptOffset.Value;
            filter &= Builders<MongoOutboxDocument>.Filter.Where(r => r.LastAttemptDate == null || r.LastAttemptDate < nextAttempt);
        }
        if (findOptions.Filter.Status.HasValue)
        {
            filter &= Builders<MongoOutboxDocument>.Filter.Eq(r => r.Status, findOptions.Filter.Status.Value);
        }

        if (findOptions.Filter.Unlocked.HasValue)
        {
            filter &= Builders<MongoOutboxDocument>.Filter.Where(r => r.LockUntil == null || r.LockUntil < DateTime.UtcNow);
        }

        if (findOptions.Filter.MessageTypeName.HasValue)
        {
            filter &= Builders<MongoOutboxDocument>.Filter.Where(r => r.MessageTypeName == findOptions.Filter.MessageTypeName.Value);
        }
        return filter;
    }
    #endregion
}
