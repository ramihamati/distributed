using ComX.Infrastructure.Distributed.Outbox;
using ComX.Infrastructure.Distributed.Outbox.store.sql;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsConfiguratorSql
{
    public static void UseUrfStore(
        this IConfiguratorStore configurator,
        Action<ConfiguratorUrfStore> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        Type outboxStorageType = typeof(IOutboxStorage);

        if (services.Any(r => r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage, OutboxStorageEntityFramework>();

        ConfiguratorUrfStore sqlStoreConfigurator = new(services);
        storeConfigurator(sqlStoreConfigurator);

        if (!sqlStoreConfigurator.ConnectionInitializedOnce)
        {
            throw new Exception($@"You are using sql store for the outbox but not 
connection was initialized.");
        }
    }

    public static void UseSqlStore(
        this IConfiguratorStore configurator,
        Action<ConfiguratorSqlStore> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        Type outboxStorageType = typeof(IOutboxStorage);

        if (services.Any(r=> r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage, OutboxStorageEntityFramework>();

        ConfiguratorSqlStore sqlStoreConfigurator = new(services);
        storeConfigurator(sqlStoreConfigurator);

        if (!sqlStoreConfigurator.ConnectionInitializedOnce)
        {
            throw new Exception($@"You are using sql store for the outbox but not 
connection was initialized.");
        }
    }

    public static void UseSqliteStore(
        this IConfiguratorStore configurator,
        Action<ConfiguratorSqliteStore> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        Type outboxStorageType = typeof(IOutboxStorage);

        if (services.Any(r => r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage, OutboxStorageEntityFramework>();

        ConfiguratorSqliteStore sqlStoreConfigurator = new(services);
        storeConfigurator(sqlStoreConfigurator);

        if (!sqlStoreConfigurator.ConnectionInitializedOnce)
        {
            throw new Exception($@"You are using sqlite store for the outbox but not 
connection was initialized.");
        }
    }
}
