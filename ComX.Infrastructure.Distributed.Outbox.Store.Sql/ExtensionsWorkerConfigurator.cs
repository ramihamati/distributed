using ComX.Infrastructure.Distributed.Outbox;
using ComX.Infrastructure.Distributed.Outbox.store.sql;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsWorkerConfigurator
{
    public static void UseUrfStore<TMessageLog>(
        this IConfiguratorWorkerStore<TMessageLog> configurator,
        Action<ConfiguratorUrfWorkerStore> storeConfigurator)
            where TMessageLog : class, IIntegrationMessageLog
    {
        IServiceCollection services = configurator.Context.ContainerServices
            ?? throw new NullReferenceException("The context does not have the services collection");

        ConfiguratorUrfWorkerStore sqlStoreConfigurator = new(configurator.Context);
        storeConfigurator(sqlStoreConfigurator);
    }

    public static void UseSqlStore<TMessageLog>(
        this IConfiguratorWorkerStore<TMessageLog> configurator,
        Action<ConfiguratorSqlWorkerStore> storeConfigurator)
            where TMessageLog : class, IIntegrationMessageLog
    {
        IServiceCollection services = configurator.Context.ContainerServices
            ?? throw new NullReferenceException("The context does not have the services collection");

        ConfiguratorSqlWorkerStore sqlStoreConfigurator = new(configurator.Context);
        storeConfigurator(sqlStoreConfigurator);
    }

    public static void UseSqliteStore<TMessageLog>(
        this IConfiguratorWorkerStore<TMessageLog> configurator,
        Action<ConfiguratorSqliteWorkerStore> storeConfigurator)
            where TMessageLog : class, IIntegrationMessageLog
    {
        IServiceCollection services = configurator.Context.ContainerServices
            ?? throw new NullReferenceException("The context does not have the services collection");

        ConfiguratorSqliteWorkerStore sqlStoreConfigurator = new(configurator.Context);
        storeConfigurator(sqlStoreConfigurator);
    }
}
