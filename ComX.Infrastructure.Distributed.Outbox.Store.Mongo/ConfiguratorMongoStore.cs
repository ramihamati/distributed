using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorMongoStore
{
    private readonly IServiceCollection _services;
    internal bool ConnectionInitializedOnce { get; private set; } = false;

    public ConfiguratorMongoStore(IServiceCollection services)
    {
        this._services = services;
    }

    /// <summary>
    /// <para>Registering a repository used by the internal storage. Caller is responsable for
    /// transactional commits (unit of work, etc). </para>
    /// Caller is also reponsible for the repository dependencies.
    /// Caller can use <see cref="OutboxMongoManager"/> which is injected in services to create the repository
    /// </summary>
    public ConfiguratorMongoStore UseRepository<TRepository>()
        where TRepository : class, IOutboxRepository
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception("The store repository was already configured");
        }
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _services.TryAddScoped<IOutboxRepository, TRepository>();
        ConnectionInitializedOnce = true;
        return this;
    }

    public ConfiguratorMongoStore UseBuiltInRepository()
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception("The store repository was already configured");
        }
        // we don't have a unit of work internally because we are not creating the context.
        // the user manages that externally
        _services.TryAddScoped<IOutboxRepository>(sp =>
        {
            OutboxMongoManager manager = sp.GetRequiredService<OutboxMongoManager>();
            IOutboxMongoSettings settings = sp.GetRequiredService<IOutboxMongoSettings>();

            return new OutboxMongoRepository<MongoOutboxDocument>(
                manager.GetCollection<MongoOutboxDocument>(settings.DbName, settings.CollectionName),
                MessageConverter.ToMongoIntegrationMessageLog,
                MessageConverter.ToIntegrationMessageLog);
        });

        ConnectionInitializedOnce = true;
        return this;
    }
}
