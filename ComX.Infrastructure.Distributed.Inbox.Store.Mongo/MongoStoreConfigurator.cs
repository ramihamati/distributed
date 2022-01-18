using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo;

public class MongoStoreConfigurator
{
    private readonly ConfiguratorContext _context;
    private readonly MongoCollectionInfoRegistry _registry;

    internal bool ConnectionInitializedOnce { get; private set; } = false;

    public MongoStoreConfigurator(
        ConfiguratorContext context,
        MongoCollectionInfoRegistry registry)
    {
        _context = context;
        _registry = registry;
    }

    public MongoStoreConfigurator UseCollection<TModel>(string dbName, string collection)
        where TModel : class
    {
        _registry.RegisterCollectionInfo(typeof(TModel), dbName, collection);
        _context.Services.TryAddScoped<IStore<TModel>, MongoStore<TModel>>();
        return this;
    }

    public MongoStoreConfigurator UseConnection<TConnection>()
        where TConnection : class, IMongoConfiguration
    {
        if (ConnectionInitializedOnce)
        {
            throw new Exception("The mongo connection was already initialized");
        }

        _context.Services.TryAddScoped<IMongoConfiguration, TConnection>();
        ConnectionInitializedOnce = true;
        return this;
    }
}
