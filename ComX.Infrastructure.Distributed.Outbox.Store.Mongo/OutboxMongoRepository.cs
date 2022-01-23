using MongoDB.Driver;

namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// Builtin repository for <typeparamref name="TMessageLog"/>. It converts it into the mongo entity <typeparamref name="TMongoMessageLog"/>
/// </summary>
/// <typeparam name="TMongoMessageLog">mongo entity converted from TMessageLog</typeparam>
/// <typeparam name="TMessageLog">oritinal repository entity</typeparam>
public class OutboxMongoRepository<TMongoMessageLog, TMessageLog> : IOutboxRepository<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
    where TMongoMessageLog : IMongoOutboxDocument
{
    private readonly IMongoCollection<TMongoMessageLog> _collection;
    private readonly Func<TMessageLog, TMongoMessageLog> _toEntity;
    private readonly Func<TMongoMessageLog, TMessageLog> _fromEntity;

    public OutboxMongoRepository(
        IMongoCollection<TMongoMessageLog> collection,
        Func<TMessageLog, TMongoMessageLog> toEntity,
        Func<TMongoMessageLog, TMessageLog> fromEntity)
    {
        _collection = collection;
        _toEntity = toEntity;
        _fromEntity = fromEntity;
    }

    public async Task DeleteAsync(TMessageLog entity, CancellationToken cancellationToken = default)
    {
        TMongoMessageLog mongoEntity = _toEntity(entity);

        FilterDefinition<TMongoMessageLog> filter =
                     Builders<TMongoMessageLog>.Filter.Eq(t => t.Id, mongoEntity.Id)
                   & Builders<TMongoMessageLog>.Filter.Eq(t => t.Timestamp, mongoEntity.Timestamp);

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
        IAsyncCursor<TMongoMessageLog> entity = await _collection.FindAsync(
            Builders<TMongoMessageLog>.Filter.Eq(r => r.Id, entityId),
            new FindOptions<TMongoMessageLog>
            {
                Limit = 1
            }, cancellationToken);

        return entity
            .ToList(cancellationToken: cancellationToken)
            .Any();
    }

    public async Task<TMessageLog> FindAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        IAsyncCursor<TMongoMessageLog> entityResponse = await _collection.FindAsync(
            Builders<TMongoMessageLog>.Filter.Eq(r => r.Id, entityId),
            new FindOptions<TMongoMessageLog>
            {
                Limit = 1
            }, cancellationToken);

        var entity = entityResponse
            .ToList(cancellationToken: cancellationToken)
            .FirstOrDefault();

        return entity is null ? null : _fromEntity(entity);
    }

    public async Task<List<TMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<TMongoMessageLog> filter = GetFilterDefinition(finder);
        FindOptions<TMongoMessageLog> findOptions = GetFindOptions(finder);

        IAsyncCursor<TMongoMessageLog> entities = await _collection.FindAsync(filter, findOptions);
        return (await entities.ToListAsync(cancellationToken)).ConvertAll(r => _fromEntity(r));
    }

    public async Task InsertAsync(TMessageLog entity, CancellationToken cancellationToken = default)
    {
        TMongoMessageLog mongoEntity = _toEntity(entity);
        mongoEntity.Id = mongoEntity.Id == Guid.Empty ? Guid.NewGuid() : mongoEntity.Id;
        mongoEntity.Timestamp = DateTime.UtcNow.Ticks;
        await _collection.InsertOneAsync(mongoEntity, cancellationToken: cancellationToken);

        // if passed argument is used by reference in other places
        entity.Timestamp = BitConverter.GetBytes(mongoEntity.Timestamp);
        entity.Id = mongoEntity.Id;
    }

    public async Task UpdateAsync(TMessageLog entity, CancellationToken cancellationToken = default)
    {
        TMongoMessageLog mongoEntity = _toEntity(entity);
        long currentTimestamp = mongoEntity.Timestamp;
        mongoEntity.Timestamp = DateTimeOffset.UtcNow.Ticks;

        FilterDefinition<TMongoMessageLog> filter =
                     Builders<TMongoMessageLog>.Filter.Eq(t => t.Id, mongoEntity.Id)
                   & Builders<TMongoMessageLog>.Filter.Eq(t => t.Timestamp, currentTimestamp);

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

    #region [ Private ]

    private static FindOptions<TMongoMessageLog> GetFindOptions(FinderMessageLog finder)
    {
        FindOptions<TMongoMessageLog> findOptions = new();

        if (finder.Limit.HasValue)
        {
            findOptions.Limit = finder.Limit.Value;
        }

        if (finder.Skip.HasValue)
        {
            findOptions.Skip = finder.Skip.Value;
        }

        findOptions.Sort = Builders<TMongoMessageLog>.Sort.Ascending(r => r.CreatedAt);

        return findOptions;
    }

    private static FilterDefinition<TMongoMessageLog> GetFilterDefinition(FinderMessageLog findOptions)
    {
        FilterDefinition<TMongoMessageLog> filter = FilterDefinition<TMongoMessageLog>.Empty;

        if (findOptions.Filter.LastAttemptOffset.HasValue)
        {
            DateTime nextAttempt = DateTime.UtcNow - findOptions.Filter.LastAttemptOffset.Value;
            filter &= Builders<TMongoMessageLog>.Filter.Where(r => r.LastAttemptDate == null || r.LastAttemptDate < nextAttempt);
        }
        if (findOptions.Filter.Status.HasValue)
        {
            filter &= Builders<TMongoMessageLog>.Filter.Eq(r => r.Status, findOptions.Filter.Status.Value);
        }

        if (findOptions.Filter.Unlocked.HasValue)
        {
            filter &= Builders<TMongoMessageLog>.Filter.Where(r => r.LockUntil == null || r.LockUntil < DateTime.UtcNow);
        }

        if (findOptions.Filter.MessageTypeName.HasValue)
        {
            filter &= Builders<TMongoMessageLog>.Filter.Where(r => r.MessageTypeName == findOptions.Filter.MessageTypeName.Value);
        }
        return filter;
    }
    #endregion
}
