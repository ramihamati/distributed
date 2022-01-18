using ComX.Infrastructure.Distributed.Outbox;
using ComX.Infrastructure.Distributed.Outbox.Masstransit;
using ComX.Infrastructure.Distributed.Workertimer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsOutboxWorker
{
    public static IServiceCollection AddOutboxWorker(
       this IServiceCollection services,
       IConfigurationOutboxWorker configuration,
       Action<ConfiguratorOutboxWorker> configurator)
    {
        services.TryAddScoped<IOutboxWorkerService, OutboxWorkerService>();

        services.TryAddScoped<IConfigurationOutboxWorker>(_ => configuration);
        services.TryAddScoped<IConfigurationTimer>(sp => sp.CreateScope().ServiceProvider.GetService<IConfigurationOutboxWorker>());

        services.AddWorkerProgramabilityTimer<OutboxPublisherWorker>(configuration);

        ConfiguratorOutboxWorker outboxWorkerConfiguration = new(services);
        configurator(outboxWorkerConfiguration);
        outboxWorkerConfiguration.EnsureAllAreRegistered();
        ValidateDeps(services);

        return services;
    }

    public static IServiceCollection AddOutboxWorker<TConfiguration>(
        this IServiceCollection services,
        Action<ConfiguratorOutboxWorker> configurator)
        where TConfiguration : class, IConfigurationOutboxWorker
    {
        services.TryAddScoped<IOutboxWorkerService, OutboxWorkerService>();

        services.TryAddScoped<IConfigurationOutboxWorker, TConfiguration>();
        services.TryAddScoped<IConfigurationTimer>(sp => sp.CreateScope().ServiceProvider.GetService<IConfigurationOutboxWorker>());

        services.AddWorkerProgramabilityTimer<TConfiguration, OutboxPublisherWorker>();

        ConfiguratorOutboxWorker outboxWorkerConfiguration = new(services);
        configurator(outboxWorkerConfiguration);
        outboxWorkerConfiguration.EnsureAllAreRegistered();
        ValidateDeps(services);

        return services;
    }

    public static IServiceCollection AddOutboxWorker<TConfiguration>(
        this IServiceCollection services,
        Action<ConfiguratorOutboxWorker> configurator,
        Func<IServiceProvider, IConfigurationOutboxWorker> configurationProvider)
        where TConfiguration : class, IConfigurationOutboxWorker
    {
        services.TryAddScoped<IOutboxWorkerService, OutboxWorkerService>();
        services.TryAddScoped<IConfigurationOutboxWorker>(sp => configurationProvider(sp.CreateScope().ServiceProvider));
        services.AddWorkerProgramabilityTimer<OutboxPublisherWorker>(configurationProvider);

        ConfiguratorOutboxWorker outboxWorkerConfiguration = new(services);
        configurator(outboxWorkerConfiguration);
        outboxWorkerConfiguration.EnsureAllAreRegistered();
        ValidateDeps(services);

        return services;
    }

    public static IServiceCollection AddOutboxWorker<TConfiguration>(
       this IServiceCollection services,
       Action<ConfiguratorOutboxWorker> configurator,
       IConfigurationOutboxWorker configuration)
       where TConfiguration : class, IConfigurationOutboxWorker
    {
        services.TryAddScoped<IOutboxWorkerService, OutboxWorkerService>();
        services.TryAddScoped<IConfigurationOutboxWorker>(_ => configuration);
        services.AddWorkerProgramabilityTimer<OutboxPublisherWorker>(configuration);

        ConfiguratorOutboxWorker outboxWorkerConfiguration = new(services);
        configurator(outboxWorkerConfiguration);
        outboxWorkerConfiguration.EnsureAllAreRegistered();
        ValidateDeps(services);

        return services;
    }

    private static void ValidateDeps(
        IServiceCollection services)
    {
        Type typeEventSerializer = typeof(IEventSerializer);

        bool hasEventSerializer = services.Any(r => r.ServiceType.Equals(typeEventSerializer));
        if (!hasEventSerializer)
        {
            throw new Exception(@$"You must register an event serializer. Use the method 
{nameof(ConfiguratorOutboxWorker)}.{nameof(ConfiguratorOutboxWorker.ConfigureSerializer)}. An event serializer is
present in ComX.Infrastructure.Distributed.Outbox.Masstransit");
        }

        Type typePublisher = typeof(IOutboxBrokerPublisher);
        bool hasPublisher = services.Any(r => r.ServiceType.Equals(typePublisher));
        if (!hasPublisher)
        {
            throw new Exception(@$"You must register a publisher. Use the method 
{nameof(ConfiguratorOutboxWorker)}.{nameof(ConfiguratorOutboxWorker.ConfigurePublisher)}. A broker publisher is
present in ComX.Infrastructure.Distributed.Outbox.Masstransit");
        }

        Type typeStorage = typeof(IOutboxStorage);
        bool hasStorage = services.Any(r => r.ServiceType.Equals(typeStorage));
        if (!hasStorage)
        {
            throw new Exception(@$"You must register a store service. Use the method 
{nameof(ConfiguratorOutboxWorker)}.{nameof(ConfiguratorOutboxWorker.ConfigureStore)}. A store is
present in ComX.Infrastructure.Distributed.Outbox.Store.Sql");
        }
    }

}
