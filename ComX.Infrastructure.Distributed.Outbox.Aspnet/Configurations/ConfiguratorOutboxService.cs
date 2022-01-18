using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace ComX.Infrastructure.Distributed.Outbox.Masstransit;

public sealed class ConfiguratorOutboxService
{
    private bool EventsRegisteredOnce { get; set; } = false;
    private bool StoreConfiguredOnce { get; set; } = false;
    private bool SerializerConfiguredOnce { get; set; } = false;

    public ConfiguratorContext Context { get; }

    public ConfiguratorOutboxService(IServiceCollection services)
    {
        Context = new ConfiguratorContext(services);

    }

    public ConfiguratorOutboxService RegisterEvents(Action<IOutboxServiceRegistryBuilder> registryBuilder)
    {
        if (EventsRegisteredOnce)
        {
            throw new Exception("The events were already registered");
        }
        EventsRegisteredOnce = true;
        OutboxServiceRegistryBuilder serviceRegistryBuilder = new();
        registryBuilder(serviceRegistryBuilder);
        IOutboxServiceRegistry eventTypeRegistry = serviceRegistryBuilder.Build();
        Context.Services.TryAddScoped<IOutboxServiceRegistry>(_ => eventTypeRegistry);

        return this;
    }

    public ConfiguratorOutboxService ConfigureStore(Action<IConfiguratorStore> storeConfigurator)
    {
        if (StoreConfiguredOnce)
        {
            throw new Exception("The store was already configured.");
        }
        StoreConfiguredOnce = true;
        IConfiguratorStore configurator = new ConfiguratorStore(Context);
        storeConfigurator(configurator);
        return this;
    }

    public ConfiguratorOutboxService ConfigureSerializer(Action<IConfiguratorEventSerializer> publisherConfigurator)
    {
        if (SerializerConfiguredOnce)
        {
            throw new Exception("The store was already configured.");
        }
        SerializerConfiguredOnce = true;
        IConfiguratorEventSerializer configurator = new ConfiguratorEventSerializer(Context);
        publisherConfigurator(configurator);
        return this;
    }

    internal void EnsureAllAreRegistered()
    {
        if (!EventsRegisteredOnce)
        {
            throw new Exception(@$"No events are registered for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxService)}.{nameof(RegisterEvents)}(...)?");
        }

        if (!StoreConfiguredOnce)
        {
            throw new Exception(@$"No store is configured for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxService)}.{nameof(ConfigureStore)}(...)?");
        }

        if (!SerializerConfiguredOnce)
        {
            throw new Exception(@$"No event serializer is configured for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxService)}.{nameof(ConfigureSerializer)}(...)?");
        }

    }
}
