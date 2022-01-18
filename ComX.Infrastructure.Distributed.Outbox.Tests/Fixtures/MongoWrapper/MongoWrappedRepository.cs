using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;
public class MongoWrappedRepository : IOutboxRepository
{
    private readonly OutboxMongoRepository<MyMongoOutboxDocument> _innerRepository;

    public MongoWrappedRepository(
        IOutboxMongoSettings outboxMongoSettings,
        OutboxMongoManager mongoManager)
    {
        _innerRepository = new OutboxMongoRepository<MyMongoOutboxDocument>(
            mongoManager.GetCollection<MyMongoOutboxDocument>(outboxMongoSettings.DbName, outboxMongoSettings.ConnectionString),
            MyMongoOutboxDocument.ToMongoIntegrationMessageLog,
            MyMongoOutboxDocument.ToIntegrationMessageLog);
    }

    public Task DeleteAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        return _innerRepository.DeleteAsync(entity, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        return _innerRepository.ExistsAsync(entityId, cancellationToken);
    }

    public Task<IntegrationMessageLog> FindAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return _innerRepository.FindAsync(entityId, cancellationToken);
    }

    public Task<List<IntegrationMessageLog>> FindAsync(
        FinderMessageLog finder,
        CancellationToken cancellationToken = default)
    {
        return _innerRepository.FindAsync(finder, cancellationToken);
    }

    public Task InsertAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        return _innerRepository.InsertAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(IntegrationMessageLog entity, CancellationToken cancellationToken = default)
    {
        return _innerRepository.UpdateAsync(entity);
    }
}
