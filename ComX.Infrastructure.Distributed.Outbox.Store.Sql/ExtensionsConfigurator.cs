using ComX.Infrastructure.Distributed.Outbox;
using ComX.Infrastructure.Distributed.Outbox.store.sql;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsConfigurator
{
    public static void UseUrfStore(
        this IConfiguratorStore configurator,
        Action<ConfiguratorUrfStore> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        ConfiguratorUrfStore sqlStoreConfigurator = new(configurator.Context);
        storeConfigurator(sqlStoreConfigurator);
    }

    public static void UseSqlStore(
        this IConfiguratorStore configurator,
        Action<ConfiguratorSqlStore> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        ConfiguratorSqlStore sqlStoreConfigurator = new(configurator.Context);
        storeConfigurator(sqlStoreConfigurator);
    }

    public static void UseSqliteStore(
        this IConfiguratorStore configurator,
        Action<ConfiguratorSqliteStore> storeConfigurator)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        ConfiguratorSqliteStore sqlStoreConfigurator = new(configurator.Context);
        storeConfigurator(sqlStoreConfigurator);
    }
}
