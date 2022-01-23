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

        AddMongoStoreBasics(storeConfigurator, configurator.Context);
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
        AddMongoStoreBasics(storeConfigurator, configurator.Context);
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
        AddMongoStoreBasics(storeConfigurator, configurator.Context);
    }

    private static void AddMongoStoreBasics(Action<ConfiguratorMongoStore> storeConfigurator, ConfiguratorContext context)
    {
        context.Services.AddScoped<OutboxMongoManager>();

        ConfiguratorMongoStore storeConfiguratorModel = new(context);
        storeConfigurator(storeConfiguratorModel);
    }
}
