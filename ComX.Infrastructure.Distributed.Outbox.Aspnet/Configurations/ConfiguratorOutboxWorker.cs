using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace ComX.Infrastructure.Distributed.Outbox.Masstransit;

public class ConfiguratorOutboxWorker
{
    private readonly ConfiguratorOutboxService _outboxServiceConfigurator;
    public ConfiguratorContext Context => _outboxServiceConfigurator.Context;

    public ConfiguratorOutboxWorker(IServiceCollection services)
    {
        _outboxServiceConfigurator = new ConfiguratorOutboxService(services);
    }

    public ConfiguratorOutboxWorker RegisterEvents(Action<IOutboxServiceRegistryBuilder> registryBuilder)
    {
        _outboxServiceConfigurator.RegisterEvents(registryBuilder);
        return this;
    }

    public ConfiguratorOutboxWorker ConfigureStore(Action<IConfiguratorStore> storeConfigurator)
    {
        _outboxServiceConfigurator.ConfigureStore(storeConfigurator);
        return this;
    }

    public ConfiguratorOutboxWorker ConfigureSerializer(Action<IConfiguratorEventSerializer> publisherConfigurator)
    {
        _outboxServiceConfigurator.ConfigureSerializer(publisherConfigurator);
        return this;
    }

    public ConfiguratorOutboxWorker ConfigurePublisher(Action<IConfiguratorPublisher> publisherConfigurator)
    {
        if (BrokerPublisherConfigured())
        {
            throw new Exception("The publisher was already configured.");
        }
        IConfiguratorPublisher configurator = new ConfiguratorPublisher(Context);
        publisherConfigurator(configurator);
        return this;
    }


    internal void EnsureAllAreRegistered()
    {
        _outboxServiceConfigurator.EnsureAllAreRegistered();
        if (!BrokerPublisherConfigured())
        {
            throw new Exception(@$"No publisher is configured for the outbox. 
Did you use the method {nameof(ConfiguratorOutboxWorker)}.{nameof(ConfiguratorOutboxWorker.ConfigurePublisher)}(...)?");
        }
    }

    private bool BrokerPublisherConfigured()
    {
        return Context.Services.Any(r => r.ServiceType.Equals(typeof(IOutboxBrokerPublisher)));
    }
}
