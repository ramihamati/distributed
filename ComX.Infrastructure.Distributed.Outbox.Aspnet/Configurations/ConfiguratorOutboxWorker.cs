using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorOutboxWorker<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    private bool EventsRegisteredOnce { get; set; } = false;
    private bool StoreConfiguredOnce { get; set; } = false;
    private bool SerializerConfiguredOnce { get; set; } = false;
    private bool PublisherConfiguredOnce { get; set; } = false;

    public ConfiguratorWorkerContext Context { get; }

    /// <summary>
    /// The worker runs in an isolated environment. The <paramref name="appServices"/> provides access to external/running app
    /// services while <paramref name="serviceSservices"/> is part of the container
    /// </summary>
    /// <param name="appServices">The external services</param>
    /// <param name="serviceSservices">Outbox worker isolated services</param>
    public ConfiguratorOutboxWorker(IServiceProvider appServices, IServiceCollection serviceSservices)
    {
        Context = new ConfiguratorWorkerContext(appServices, serviceSservices);
    }

    public ConfiguratorOutboxWorker<TMessageLog> ConfigureEvents(Action<IOutboxServiceRegistryBuilder> registryBuilder)
    {
        if (EventsRegisteredOnce)
        {
            throw new Exception("The events were already registered");
        }
        EventsRegisteredOnce = true;
        OutboxServiceRegistryBuilder serviceRegistryBuilder = new();
        registryBuilder(serviceRegistryBuilder);
        IOutboxServiceRegistry eventTypeRegistry = serviceRegistryBuilder.Build();
        Context.ContainerServices.TryAddScoped<IOutboxServiceRegistry>(_ => eventTypeRegistry);
        return this;
    }

    public ConfiguratorOutboxWorker<TMessageLog> ConfigureStore(Action<IConfiguratorWorkerStore<TMessageLog>> storeConfigurator)
    {
        if (StoreConfiguredOnce)
        {
            throw new Exception("The store was already configured.");
        }
        StoreConfiguredOnce = true;
        IConfiguratorWorkerStore<TMessageLog> configurator = new ConfiguratorWorkerStore<TMessageLog>(Context);
        storeConfigurator(configurator);
        return this;
    }

    public ConfiguratorOutboxWorker<TMessageLog> ConfigureSerializer(Action<IConfiguratorWorkerEventSerializer> publisherConfigurator)
    {
        if (SerializerConfiguredOnce)
        {
            throw new Exception("The store was already configured.");
        }
        SerializerConfiguredOnce = true;
        IConfiguratorWorkerEventSerializer configurator = new ConfiguratorWorkerEventSerializer(Context);
        publisherConfigurator(configurator);
        return this;
    }

    public ConfiguratorOutboxWorker<TMessageLog> ConfigurePublisher(Action<IConfiguratorWorkerPublisher> publisherConfigurator)
    {
        if (PublisherConfiguredOnce)
        {
            throw new Exception("The publisher was already configured.");
        }
        PublisherConfiguredOnce = true;
        IConfiguratorWorkerPublisher configurator = new ConfiguratorWorkerPublisher(Context);
        publisherConfigurator(configurator);
        return this;
    }


    internal void EnsureAllAreRegistered()
    {
        if (!EventsRegisteredOnce)
        {
            throw new Exception(@$"No events are registered for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxWorker<TMessageLog>)}.{nameof(ConfigureEvents)}(...)?");
        }

        if (!StoreConfiguredOnce)
        {
            throw new Exception(@$"No store is configured for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxWorker<TMessageLog>)}.{nameof(ConfigureStore)}(...)?");
        }

        if (!SerializerConfiguredOnce)
        {
            throw new Exception(@$"No event serializer is configured for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxWorker<TMessageLog>)}.{nameof(ConfigureSerializer)}(...)?");
        }

        if (!PublisherConfiguredOnce)
        {
            throw new Exception(@$"No publisher is configured for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxWorker<TMessageLog>)}.{nameof(ConfiguratorOutboxWorker<TMessageLog>.ConfigurePublisher)}(...)?");
        }
    }

    private bool BrokerPublisherConfigured()
    {
        return Context.ContainerServices.Any(r => r.ServiceType.Equals(typeof(IOutboxBrokerPublisher)));
    }
}
