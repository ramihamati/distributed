using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorMongoStore
{
    private readonly ConfiguratorContext _context;

    public ConfiguratorMongoStore(ConfiguratorContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// <para>Registering a repository used by the internal storage. Caller is responsable for
    /// transactional commits (unit of work, etc). </para>
    /// Caller is also reponsible for the repository dependencies.
    /// Caller can use <see cref="OutboxMongoManager"/> which is injected in services to create the repository
    /// </summary>
    public ConfiguratorMongoStore UseRepository<TRepository, TMessageLog>()
        where TMessageLog : class, IIntegrationMessageLog
        where TRepository : class, IOutboxRepository<TMessageLog>
    {
        _context.Services.TryAddScoped<IOutboxStorage<TMessageLog>, OutboxMongoStorage<TMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxRepository<TMessageLog>, TRepository>();
        return this;
    }


    /// <summary>
    /// <para>Registering a repository used by the internal storage. Caller is responsable for
    /// transactional commits (unit of work, etc). </para>
    /// Caller is also reponsible for the repository dependencies.
    /// Caller can use <see cref="OutboxMongoManager"/> which is injected in services to create the repository
    /// </summary>
    public ConfiguratorMongoStore UseRepository<TRepository>()
        where TRepository : class, IOutboxRepository<IntegrationMessageLog>
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxMongoStorage<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxRepository<IntegrationMessageLog>, TRepository>();
        return this;
    }

    /// <summary>
    /// Uses a builtin repository that handles <see cref="IntegrationMessageLog"/> using a converter to 
    /// a builtin model <see cref="MongoOutboxDocument"/>
    /// </summary>
    public ConfiguratorMongoStore UseBuiltInRepository<TMessageLog>(
        Func<MongoOutboxDocument, TMessageLog> converterFromDocument,
        Func<TMessageLog, MongoOutboxDocument> converterToDocument)
        where TMessageLog : class, IIntegrationMessageLog
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxMongoStorage<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxRepository<TMessageLog>>(sp =>
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
    public ConfiguratorMongoStore UseBuiltInRepository()
    {
        _context.Services.TryAddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxMongoStorage<IntegrationMessageLog>>();
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _context.Services.TryAddScoped<IOutboxRepository<IntegrationMessageLog>>(sp =>
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
