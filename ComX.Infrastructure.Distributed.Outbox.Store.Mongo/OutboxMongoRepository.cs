using MongoDB.Driver;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxMongoRepository<TEntity> : IOutboxRepository
    where TEntity : IMongoOutboxDocument
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly Func<IntegrationMessageLog, TEntity> _toEntity;
    private readonly Func<TEntity, IntegrationMessageLog> _fromEntity;

    public OutboxMongoRepository(
        IMongoCollection<TEntity> collection,
        Func<IntegrationMessageLog, TEntity> toEntity,
        Func<TEntity, IntegrationMessageLog> fromEntity)
    {
        _collection = collection;
        _toEntity = toEntity;
        _fromEntity = fromEntity;
    }

    public async Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        TEntity mongoEntity = _toEntity(entity);

        FilterDefinition<TEntity> filter =
                     Builders<TEntity>.Filter.Eq(t => t.Id, mongoEntity.Id)
                   & Builders<TEntity>.Filter.Eq(t => t.Timestamp, mongoEntity.Timestamp);

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
        IAsyncCursor<TEntity> entity = await _collection.FindAsync(
            Builders<TEntity>.Filter.Eq(r => r.Id, entityId),
            new FindOptions<TEntity>
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
        IAsyncCursor<TEntity> entityResponse = await _collection.FindAsync(
            Builders<TEntity>.Filter.Eq(r => r.Id, entityId),
            new FindOptions<TEntity>
            {
                Limit = 1
            }, cancellationToken);

        var entity = entityResponse
            .ToList(cancellationToken: cancellationToken)
            .FirstOrDefault();

        return entity is null ? null : _fromEntity(entity);
    }

    public async Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<TEntity> filter = GetFilterDefinition(finder);
        FindOptions<TEntity> findOptions = GetFindOptions(finder);

        IAsyncCursor<TEntity> entities = await _collection.FindAsync(filter, findOptions);
        return (await entities.ToListAsync(cancellationToken)).ConvertAll(r => _fromEntity(r));
    }

    public async Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        TEntity mongoEntity = _toEntity(entity);
        mongoEntity.Id = mongoEntity.Id == Guid.Empty ? Guid.NewGuid() : mongoEntity.Id;
        mongoEntity.Timestamp = DateTime.UtcNow.Ticks;
        await _collection.InsertOneAsync(mongoEntity, cancellationToken: cancellationToken);

        // if passed argument is used by reference in other places
        entity.Timestamp = BitConverter.GetBytes(mongoEntity.Timestamp);
        entity.Id = mongoEntity.Id;
    }

    public async Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        TEntity mongoEntity = _toEntity(entity);
        long currentTimestamp = mongoEntity.Timestamp;
        mongoEntity.Timestamp = DateTimeOffset.UtcNow.Ticks;

        FilterDefinition<TEntity> filter =
                     Builders<TEntity>.Filter.Eq(t => t.Id, mongoEntity.Id)
                   & Builders<TEntity>.Filter.Eq(t => t.Timestamp, currentTimestamp);

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

    private static FindOptions<TEntity> GetFindOptions(FinderMessageLog finder)
    {
        FindOptions<TEntity> findOptions = new();

        if (finder.Limit.HasValue)
        {
            findOptions.Limit = finder.Limit.Value;
        }

        if (finder.Skip.HasValue)
        {
            findOptions.Skip = finder.Skip.Value;
        }

        findOptions.Sort = Builders<TEntity>.Sort.Ascending(r => r.CreatedAt);

        return findOptions;
    }

    private static FilterDefinition<TEntity> GetFilterDefinition(FinderMessageLog findOptions)
    {
        FilterDefinition<TEntity> filter = FilterDefinition<TEntity>.Empty;

        if (findOptions.Filter.LastAttemptOffset.HasValue)
        {
            DateTime nextAttempt = DateTime.UtcNow - findOptions.Filter.LastAttemptOffset.Value;
            filter &= Builders<TEntity>.Filter.Where(r => r.LastAttemptDate == null || r.LastAttemptDate < nextAttempt);
        }
        if (findOptions.Filter.Status.HasValue)
        {
            filter &= Builders<TEntity>.Filter.Eq(r => r.Status, findOptions.Filter.Status.Value);
        }

        if (findOptions.Filter.Unlocked.HasValue)
        {
            filter &= Builders<TEntity>.Filter.Where(r => r.LockUntil == null || r.LockUntil < DateTime.UtcNow);
        }

        if (findOptions.Filter.MessageTypeName.HasValue)
        {
            filter &= Builders<TEntity>.Filter.Where(r => r.MessageTypeName == findOptions.Filter.MessageTypeName.Value);
        }
        return filter;
    }
    #endregion
}
