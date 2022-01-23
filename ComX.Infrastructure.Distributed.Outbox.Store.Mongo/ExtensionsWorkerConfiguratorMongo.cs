using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace ComX.Infrastructure.Distributed.Outbox;

public static class ExtensionsWorkerConfiguratorMongo
{
    public static void UseMongoStore<TConfiguration, TMessageLog>(
        this IConfiguratorWorkerStore<TMessageLog> configurator,
        Action<ConfiguratorMongoWorkerStore> storeConfigurator)
            where TConfiguration : class, IOutboxMongoSettings
            where TMessageLog : class, IIntegrationMessageLog
    {
        IServiceCollection containerServices = configurator.Context.ContainerServices
            ?? throw new NullReferenceException("The context does not have the services collection");

        containerServices.TryAddScoped<IOutboxMongoSettings>(sp =>
        {
            return configurator.Context.AppServices.GetService<TConfiguration>()
                ?? ActivatorUtilities.CreateInstance<TConfiguration>(sp);
        });

        AddMongoStoreBasics(storeConfigurator, configurator.Context);
    }

    public static void UseMongoStore<TConfiguration, TMessageLog>(
       this IConfiguratorWorkerStore<TMessageLog> configurator,
       TConfiguration configuration,
       Action<ConfiguratorMongoWorkerStore> storeConfigurator)
           where TConfiguration : class, IOutboxMongoSettings
           where TMessageLog : class, IIntegrationMessageLog
    {
        IServiceCollection containerServices = configurator.Context.ContainerServices
            ?? throw new NullReferenceException("The context does not have the services collection");
        containerServices.TryAddScoped<IOutboxMongoSettings>(_ => configuration);
        AddMongoStoreBasics(storeConfigurator, configurator.Context);
    }

    public static void UseMongoStore<TConfiguration, TMessageLog>(
        this IConfiguratorWorkerStore<TMessageLog> configurator,
        Func<IServiceProvider, IOutboxMongoSettings> configurationFactory,
        Action<ConfiguratorMongoWorkerStore> storeConfigurator)
            where TConfiguration : class, IOutboxMongoSettings
            where TMessageLog : class, IIntegrationMessageLog
    {
        IServiceCollection services = configurator.Context.ContainerServices
            ?? throw new NullReferenceException("The context does not have the services collection");
        
        services.AddScoped(_ =>
        {
            return configurationFactory(configurator.Context.AppServices);
        });

        AddMongoStoreBasics(storeConfigurator, configurator.Context);
    }

    private static void AddMongoStoreBasics(Action<ConfiguratorMongoWorkerStore> storeConfigurator, ConfiguratorWorkerContext context)
    {
        context.ContainerServices.AddScoped<OutboxMongoManager>();

        ConfiguratorMongoWorkerStore storeConfiguratorModel = new(context);
        storeConfigurator(storeConfiguratorModel);
    }
}
