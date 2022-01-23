using ComX.Infrastructure.Distributed.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsPublisherConfigurator
{
    public static void UseMassTransitPublisher(
        this IConfiguratorWorkerPublisher brokerConfigurator)
    {
        brokerConfigurator.Context.ContainerServices.TryAddScoped<IOutboxBrokerPublisher, OutboxMassTransitPublisher>();
    }

    public static void UseMassTransitMediatorPublisher(
        this IConfiguratorWorkerPublisher brokerConfigurator)
    {
        brokerConfigurator.Context.ContainerServices.TryAddScoped<IOutboxBrokerPublisher, OutboxMassTransitMediatorPublisher>();
    }
}

