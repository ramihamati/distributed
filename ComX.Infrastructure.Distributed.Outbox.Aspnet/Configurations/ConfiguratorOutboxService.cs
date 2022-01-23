using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace ComX.Infrastructure.Distributed.Outbox;

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

    public ConfiguratorOutboxService ConfigureTransforms(Action<ITransformerServiceConfigurator> configurator)
    {
        TransformerServiceConfigurator serviceConfigurator = new(Context);
        configurator(serviceConfigurator);
        // if no transformer is added

        bool hasTransformer =
            Context.Services.Any(r => r.ServiceType == typeof(IOutboxTransformer));

        if (!hasTransformer && serviceConfigurator.Transforms.Count > 0)
        {
            throw new Exception("Detected registered transform requirements but no transformer. You can use UseAutoMapperTransform()");
        }

        Context.Services.AddScoped<IOutboxTransformerService, TransformerService>(sp =>
        {
            return ActivatorUtilities.CreateInstance<TransformerService>(sp, new object[]
            {
                serviceConfigurator.Transforms
            });
        });

        // if no transformer is added, and no transforms registered so no exception is thrown
        // then we add a Noop one which is not used, but required bu the ITransformerService in the ctor
        Context.Services.TryAddScoped<IOutboxTransformer, NoopTransformer>();
        return this;
    }

    public ConfiguratorOutboxService ConfigureEvents(Action<IOutboxServiceRegistryBuilder> registryBuilder)
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
Did you use the method {nameof(ConfiguratorOutboxService)}.{nameof(ConfigureEvents)}(...)?");
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
