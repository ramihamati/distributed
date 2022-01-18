using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo;

public static class ExtensionsMongoConfigurator
{
    public static void UseMongoRepository(
        this IStoreConfigurator configurator,
        Action<MongoStoreConfigurator> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        MongoCollectionInfoRegistry registry = new();
        services.TryAddScoped<IStoreProvider, MongoStoreProvider>();
        services.TryAddScoped<MongoCollectionInfoRegistry>(_ => registry);
        services.TryAddScoped<MongoManager>();

        MongoStoreConfigurator mongoStoreConfigurator = new(
            configurator.Context,
            registry);

        storeConfigurator(mongoStoreConfigurator);

        if (!mongoStoreConfigurator.ConnectionInitializedOnce)
        {
            throw new Exception($@"You are using mongo store for the inbox but not mongo 
connection was initialized. Use the method {nameof(MongoStoreConfigurator)}.{nameof(MongoStoreConfigurator.UseConnection)}");
        }
    }
}
