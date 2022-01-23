using ComX.Infrastructure.Distributed.Outbox;
using ComX.Infrastructure.Distributed.Workertimer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsOutboxWorker
{
    public static IServiceCollection AddOutboxWorker<TConfiguration, TMessageLog>(
        this IServiceCollection services,
        Action<ConfiguratorOutboxWorker<TMessageLog>> configurator)
            where TConfiguration : class, IConfigurationOutboxWorker
            where TMessageLog : class, IIntegrationMessageLog
    {
        AddWorkerServices<TMessageLog>(
            services,
            (appServices, moduleServices) =>
            {
                moduleServices.AddScoped<IConfigurationOutboxWorker, TConfiguration>(isp =>
                    appServices.GetService<TConfiguration>() ?? ActivatorUtilities.CreateInstance<TConfiguration>(isp));
                moduleServices.TryAddScoped<IConfigurationTimer>(sp =>
                    appServices.CreateScope().ServiceProvider.GetService<IConfigurationOutboxWorker>());
            },
            configurator);

        // we can add multiple workers as long as 
        // OutboxPublisherWorker<TMessageLog> is different. This represents a process added once
        // in the services
        services.AddWorkerProgramabilityTimer<TConfiguration, OutboxPublisherWorker<TMessageLog>>();
        return services;
    }

    public static IServiceCollection AddOutboxWorker<TConfiguration>(
       this IServiceCollection services,
       TConfiguration configuration,
       Action<ConfiguratorOutboxWorker<IntegrationMessageLog>> configurator)
         where TConfiguration : class, IConfigurationOutboxWorker
    {
        AddWorkerServices<IntegrationMessageLog>(
           services,
           (appServices, moduleServices) =>
           {
               moduleServices.TryAddScoped<TConfiguration>(_ => configuration);
               moduleServices.TryAddScoped<IConfigurationTimer>(sp
                   => sp.CreateScope().ServiceProvider.GetService<TConfiguration>());
           },
           configurator);

        // we can add multiple workers as long as 
        // OutboxPublisherWorker<TMessageLog> is different. This represents a process added once
        // in the services
        services.AddWorkerProgramabilityTimer<OutboxPublisherWorker<IntegrationMessageLog>>(configuration);
        return services;
    }

    public static IServiceCollection AddOutboxWorker<TConfiguration>(
        this IServiceCollection services,
        Action<ConfiguratorOutboxWorker<IntegrationMessageLog>> configurator)
        where TConfiguration : class, IConfigurationOutboxWorker
    {
        AddWorkerServices<IntegrationMessageLog>(
          services,
          (appServices, moduleServices) =>
          {
              moduleServices.AddScoped<IConfigurationOutboxWorker, TConfiguration>(isp =>
              {
                  return appServices.GetService<TConfiguration>() ?? ActivatorUtilities.CreateInstance<TConfiguration>(isp);
              });
              moduleServices.TryAddScoped<IConfigurationTimer>(sp => sp.CreateScope().ServiceProvider.GetService<IConfigurationOutboxWorker>());

          },
          configurator);

        // we can add multiple workers as long as 
        // OutboxPublisherWorker<TMessageLog> is different. This represents a process added once
        // in the services
        services.AddWorkerProgramabilityTimer<TConfiguration, OutboxPublisherWorker<IntegrationMessageLog>>();
        return services;
    }

    public static IServiceCollection AddOutboxWorker<TConfiguration>(
        this IServiceCollection services,
        Action<ConfiguratorOutboxWorker<IntegrationMessageLog>> configurator,
        Func<IServiceProvider, TConfiguration> configurationProvider)
            where TConfiguration : class, IConfigurationOutboxWorker
    {
        AddWorkerServices<IntegrationMessageLog>(
          services,
          (appServices, moduleServices) =>
          {
              moduleServices.AddScoped<TConfiguration>(isp =>
              {
                  return configurationProvider(appServices.CreateScope().ServiceProvider);
              });
              moduleServices.TryAddScoped<IConfigurationTimer>(sp => sp.CreateScope().ServiceProvider.GetService<TConfiguration>());
          },
          configurator);

        // we can add multiple workers as long as 
        // OutboxPublisherWorker<TMessageLog> is different. This represents a process added once
        // in the services
        services.AddWorkerProgramabilityTimer<OutboxPublisherWorker<IntegrationMessageLog>>(configurationProvider);
        return services;
    }

    private static void AddWorkerServices<TMessageLog>(
        IServiceCollection services,
        Action<IServiceProvider, IServiceCollection> configureModuleServices,
        Action<ConfiguratorOutboxWorker<TMessageLog>> configurator)
        where TMessageLog : class, IIntegrationMessageLog
    {
        // isolating worker services because we want multiple worker processes in the same app
        // and different definitions collide

        services.AddScoped<OutboxPublisherWorker<TMessageLog>>(sp =>
        {
            IServiceCollection serviceServices = new ServiceCollection();

            // copy services
            // TODO: how does DI create new instances of ILogger<T>. Copying LoggerFactory and Logger's in new services
            // won't work
            serviceServices.AddScoped<ILogger<IOutboxService>>(_ =>
                sp.CreateScope().ServiceProvider.GetService<ILoggerFactory>().CreateLogger<IOutboxService>());
            serviceServices.AddScoped<ILogger<OutboxPublisherWorker<TMessageLog>>>(_ =>
                sp.CreateScope().ServiceProvider.GetService<ILoggerFactory>().CreateLogger<OutboxPublisherWorker<TMessageLog>>());
            serviceServices.AddScoped<IConfiguration>(isp => sp.GetService<IConfiguration>());
            // end copy services

            configureModuleServices(sp, serviceServices);

            serviceServices.AddScoped<IOutboxWorkerService<TMessageLog>, OutboxWorkerService<TMessageLog>>();
            serviceServices.AddScoped<OutboxPublisherWorker<TMessageLog>>();

            ConfiguratorOutboxWorker<TMessageLog> outboxWorkerConfiguration = new(sp, serviceServices);
            configurator(outboxWorkerConfiguration);
            outboxWorkerConfiguration.EnsureAllAreRegistered();
            ValidateDeps(serviceServices);

            IServiceProvider serviceProvider = serviceServices.BuildServiceProvider();

            return serviceProvider.GetService<OutboxPublisherWorker<TMessageLog>>();
        });
    }

    private static void ValidateDeps(
        IServiceCollection services)
    {
        Type typeEventSerializer = typeof(IEventSerializer);

        bool hasEventSerializer = services.Any(r => r.ServiceType.Equals(typeEventSerializer));
        if (!hasEventSerializer)
        {
            throw new Exception(@$"You must register an event serializer. Use the method 
{nameof(ConfiguratorOutboxWorker<IntegrationMessageLog>)}.{nameof(ConfiguratorOutboxWorker<IntegrationMessageLog>.ConfigureSerializer)}. An event serializer is
present in ComX.Infrastructure.Distributed.Outbox.Masstransit");
        }

        Type typePublisher = typeof(IOutboxBrokerPublisher);
        bool hasPublisher = services.Any(r => r.ServiceType.Equals(typePublisher));
        if (!hasPublisher)
        {
            throw new Exception(@$"You must register a publisher. Use the method 
{nameof(ConfiguratorOutboxWorker<IntegrationMessageLog>)}.{nameof(ConfiguratorOutboxWorker<IntegrationMessageLog>.ConfigurePublisher)}. A broker publisher is
present in ComX.Infrastructure.Distributed.Outbox.Masstransit");
        }

        //        Type typeStorage = typeof(IOutboxStorage);
        //        bool hasStorage = services.Any(r => r.ServiceType.Equals(typeStorage));
        //        if (!hasStorage)
        //        {
        //            throw new Exception(@$"You must register a store service. Use the method 
        //{nameof(ConfiguratorOutboxWorker)}.{nameof(ConfiguratorOutboxWorker.ConfigureStore)}. A store is
        //present in ComX.Infrastructure.Distributed.Outbox.Store.Sql");
        //        }
    }

}
