using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace ComX.Infrastructure.Distributed.Outbox;

public static class ExtensionsConfiguratorMongo
{
    public static void UseMongoStore<TConfiguration>(
        this IConfiguratorStore configurator,
        Action<ConfiguratorMongoStore> storeConfigurator)
        where TConfiguration : class, IOutboxMongoSettings
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        services.TryAddScoped<IOutboxMongoSettings, TConfiguration>();

        AddMongoStoreBasics(storeConfigurator, services);
    }

    public static void UseMongoStore<TConfiguration>(
       this IConfiguratorStore configurator,
       TConfiguration configuration,
       Action<ConfiguratorMongoStore> storeConfigurator)
       where TConfiguration : class, IOutboxMongoSettings
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");
        services.TryAddScoped<IOutboxMongoSettings>(_ => configuration);
        AddMongoStoreBasics(storeConfigurator, services);
    }

    public static void UseMongoStore<TConfiguration>(
        this IConfiguratorStore configurator,
        Func<IServiceProvider, IOutboxMongoSettings> configurationFactory,
        Action<ConfiguratorMongoStore> storeConfigurator)
        where TConfiguration : class, IOutboxMongoSettings
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");
        services.TryAddScoped(configurationFactory);
        AddMongoStoreBasics(storeConfigurator, services);
    }

    private static void AddMongoStoreBasics(Action<ConfiguratorMongoStore> storeConfigurator, IServiceCollection services)
    {
        Type outboxStorageType = typeof(IOutboxStorage);

        if (services.Any(r => r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage, OutboxMongoStorage>();
        services.AddScoped<OutboxMongoManager>();

        ConfiguratorMongoStore storeConfiguratorModel = new(services);
        storeConfigurator(storeConfiguratorModel);

        if (!storeConfiguratorModel.ConnectionInitializedOnce)
        {
            throw new Exception(@"You are using mongo store for the outbox but not 
repository type was configured.");
        }
    }
}
