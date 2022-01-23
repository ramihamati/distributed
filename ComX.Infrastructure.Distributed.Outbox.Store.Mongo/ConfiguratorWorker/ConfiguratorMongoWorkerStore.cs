using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorMongoWorkerStore
{
    private readonly ConfiguratorWorkerContext _context;

    public ConfiguratorMongoWorkerStore(ConfiguratorWorkerContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Uses a builtin repository that handles <see cref="IntegrationMessageLog"/> using a converter to 
    /// a builtin model <see cref="MongoOutboxDocument"/>
    /// </summary>
    public ConfiguratorMongoWorkerStore UseBuiltInRepository<TMessageLog>(
        Func<MongoOutboxDocument, TMessageLog> converterFromDocument,
        Func<TMessageLog, MongoOutboxDocument> converterToDocument)
        where TMessageLog : class, IIntegrationMessageLog
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxMongoStorage<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxRepository<TMessageLog>>(sp =>
        {
            OutboxMongoManager manager = sp.GetRequiredService<OutboxMongoManager>();
            IOutboxMongoSettings settings = sp.GetRequiredService<IOutboxMongoSettings>();

            return new OutboxMongoRepository<MongoOutboxDocument, TMessageLog>(
                manager.GetCollection<MongoOutboxDocument>(settings.DbName, settings.CollectionName),
                converterToDocument,
                converterFromDocument);
        });

        return this;
    }

    /// <summary>
    /// Uses a builtin repository that handles <see cref="IntegrationMessageLog"/> using a converter to 
    /// a builtin model <see cref="MongoOutboxDocument"/>
    /// </summary>
    public ConfiguratorMongoWorkerStore UseBuiltInRepository()
    {
        _context.ContainerServices.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxMongoStorage<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.ContainerServices.TryAddScoped<IOutboxRepository<IntegrationMessageLog>>(sp =>
        {
            OutboxMongoManager manager = sp.GetRequiredService<OutboxMongoManager>();
            IOutboxMongoSettings settings = sp.GetRequiredService<IOutboxMongoSettings>();

            return new OutboxMongoRepository<MongoOutboxDocument, IntegrationMessageLog>(
                manager.GetCollection<MongoOutboxDocument>(settings.DbName, settings.CollectionName),
                MessageConverter.ToMongoIntegrationMessageLog,
                MessageConverter.ToIntegrationMessageLog);
        });

        return this;
    }
}
