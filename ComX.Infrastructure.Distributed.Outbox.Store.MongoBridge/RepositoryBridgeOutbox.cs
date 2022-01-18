using ComX.Common.MongoBase;
using MongoDB.Driver;

namespace ComX.Infrastructure.Distributed.Outbox.Store.MongoBridge;

/// Kepping this class generic is not a mistake. Do not add <see cref="IntegrationMessageEntity"/> directly. We want this 
/// class to be flexible and user to use any entity if he wants
public class RepositoryBridgeOutbox<TEntity> : IOutboxRepository
     where TEntity : BaseDocument<TEntity>, IMongoOutboxDocument
{
    private readonly ICollectionOptimistic<TEntity> _collection;
    private readonly Func<IntegrationMessageLog, TEntity> _toEntity;
    private readonly Func<TEntity, IntegrationMessageLog> _fromEntity;
    private readonly IOutboxSessionWrapper _session;

    public RepositoryBridgeOutbox(
        ICollectionOptimistic<TEntity> collection,
        Func<IntegrationMessageLog, TEntity> toEntity,
        Func<TEntity, IntegrationMessageLog> fromEntity,
        IOutboxSessionWrapper session)
    {
        _collection = collection;
        _toEntity = toEntity;
        _fromEntity = fromEntity;
        _session = session;
    }

    public async Task DeleteAsync(
        IntegrationMessageLog entity,
        CancellationToken cancellationToken = default)
    {
        TEntity mongoEntity = _toEntity(entity);
        await _collection.DeleteOneAsync(mongoEntity, _session.ActiveSession);
    }

    public async Task<bool> ExistsAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _collection.ExistsAsync(entityId, _session.ActiveSession);
    }

    public async Task<IntegrationMessageLog> FindAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        TEntity? entity = await _collection.FindOneByIdAsync(entityId, _session.ActiveSession);
        return _fromEntity(entity);
    }

    public async Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        FilterDefinition<TEntity> filter = GetFilterDefinition(finder);
        QueryFindOptions<TEntity> findOptions = GetFindOptions(finder);

        IAsyncEnumerable<TEntity>? entities = await _collection.FindManyAsync(filter, findOptions, _session.ActiveSession);
        return (await entities.ToListAsync()).ConvertAll(r => _fromEntity(r));
    }

    public async Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        TEntity mongoEntity = _toEntity(entity);
        mongoEntity.Id = mongoEntity.Id == Guid.Empty ? Guid.NewGuid() : mongoEntity.Id;
        mongoEntity.Timestamp = DateTime.UtcNow.Ticks;
        await _collection.InsertOneAsync(mongoEntity, _session.ActiveSession);

        // if passed argument is used by reference in other places
        entity.Timestamp = BitConverter.GetBytes(mongoEntity.Timestamp);
        entity.Id = mongoEntity.Id;
    }

    public async Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        TEntity mongoEntity = _toEntity(entity);
        await _collection.ReplaceOneAsync(mongoEntity, _session.ActiveSession);
        // if passed argument is used by reference in other places
        entity.Timestamp = BitConverter.GetBytes(mongoEntity.Timestamp);
    }

    #region [ Private ]

    private static QueryFindOptions<TEntity> GetFindOptions(FinderMessageLog finder)
    {
        QueryFindOptions<TEntity> findOptions = new();

        if (finder.Limit.HasValue)
        {
            findOptions.Limit = finder.Limit.Value;
        }

        if (finder.Skip.HasValue)
        {
            findOptions.Skip = finder.Skip.Value;
        }

        findOptions.AddSortAscending(r => r.CreatedAt);
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
