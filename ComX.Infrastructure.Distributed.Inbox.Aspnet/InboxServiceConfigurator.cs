using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Inbox.Aspnet;

public class InboxServiceConfigurator
{
    internal ConfiguratorContext Context { get; }

    private bool EventsRegisteredOnce { get; set; } = false;
    private bool BrokerConfiguredOnce { get; set; } = false;
    private bool WorkerConfiguredOnce { get; set; } = false;
    private bool TransformerConfiguredOnce { get; set; } = false;
    private bool StoreConfiguredOnce { get; set; } = false;

    public InboxServiceConfigurator(IServiceCollection services)
    {
        Context = new ConfiguratorContext(services);
    }

    /// <summary>
    /// Register the events which will be used to create consumers
    /// </summary>
    public InboxServiceConfigurator RegisterEvents(Action<IEventTypeRegistryBuilder> builder)
    {
        if (EventsRegisteredOnce)
        {
            throw new Exception("The events were already registered");
        }

        builder(Context.EventTypeRegistryBuilder);
        EventsRegisteredOnce = true;
        return this;
    }

    /// <summary>
    /// Select a type of broker message system
    /// </summary>
    public InboxServiceConfigurator ConfigureBroker(Action<IBrokerConfigurator> configurator)
    {
        if (BrokerConfiguredOnce)
        {
            throw new Exception("The broker was already configured");
        }
        BrokerConfigurator brokerConfigurator = new (Context);
        configurator(brokerConfigurator);
        BrokerConfiguredOnce = true;

        if (brokerConfigurator.Context.BusConfigurator is null)
        {
            throw new Exception("The bus was configured but no bus configurator was detected");
        }
        return this;
    }

    /// <summary>
    /// Select a type of worker to run the consumers in
    /// </summary>
    public InboxServiceConfigurator ConfigureWorker(Action<IWorkerConfigurator> configurator)
    {
        if (WorkerConfiguredOnce)
        {
            throw new Exception("The worker was already configured");
        }
        WorkerConfigurator workerConfigurator = new (Context);
        configurator(workerConfigurator);
        WorkerConfiguredOnce = true;
        return this;
    }

    public InboxServiceConfigurator ConfigureTransforms(Action<ITransformerServiceConfigurator> configurator)
    {
        if (TransformerConfiguredOnce)
        {
            throw new Exception("The transformer was already configured");
        }
        TransformerServiceConfigurator serviceConfigurator = new(Context);
        configurator(serviceConfigurator);
        // if no transformer is added

        bool hasTransformer =
            Context.Services.Any(r => r.ServiceType == typeof(ITransformer));

        if (!hasTransformer && serviceConfigurator.Transforms.Count > 0)
        {
            throw new Exception("Detected registered transform requirements but no transformer. You can use UseAutoMapperTransform()");
        }

        Context.Services.AddScoped<ITransformerService, TransformerService>(sp =>
        {
            return ActivatorUtilities.CreateInstance<TransformerService>(sp, new object[]
            {
                serviceConfigurator.Transforms
            });
        });

        // if no transformer is added, and no transforms registered so no exception is thrown
        // then we add a Noop one which is not used, but required bu the ITransformerService in the ctor
        Context.Services.TryAddScoped<ITransformer, NoopTransformer>();
        TransformerConfiguredOnce = true;
        return this;
    }

    public InboxServiceConfigurator ConfigureStore(Action<IStoreConfigurator> configurator)
    {
        if (StoreConfiguredOnce)
        {
            throw new Exception("The store was already configured.");
        }
      
        StoreConfigurator storeConfigurator = new(Context);
        configurator(storeConfigurator);

        StoreConfiguredOnce = true;
        return this;
    }

    internal void EnsureAllAreRegistered()
    {
        if (!EventsRegisteredOnce)
        {
            throw new Exception(@$"No events are registered for the inbox. 
Did you use the method {nameof(InboxServiceConfigurator)}.{RegisterEvents}?");
        }

        if (!BrokerConfiguredOnce)
        {
            throw new Exception(@$"No broker is configured for the inbox. 
Did you use the method {nameof(InboxServiceConfigurator)}.{ConfigureBroker}?");
        }

        if (!WorkerConfiguredOnce)
        {
            throw new Exception(@$"No worker is configured for the inbox. 
Did you use the method {nameof(InboxServiceConfigurator)}.{ConfigureWorker}?");
        }

        if (!TransformerConfiguredOnce)
        {
            // user may decide he does not want transformations
            // we add a fake one
            ConfigureTransforms(_ => { });
        }

        if (!StoreConfiguredOnce)
        {
            throw new Exception(@$"No store is configured for the inbox. 
Did you use the method {nameof(InboxServiceConfigurator)}.{ConfigureStore}?");
        }

    }
}
