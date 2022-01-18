using ComX.Infrastructure.Distributed.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsPublisherConfigurator
{
    public static void UseMassTransitPublisher(
        this IConfiguratorPublisher brokerConfigurator)
    {
        brokerConfigurator.Context.Services.TryAddScoped<IOutboxBrokerPublisher, OutboxMassTransitPublisher>();
    }

    public static void UseMassTransitMediatorPublisher(
        this IConfiguratorPublisher brokerConfigurator)
    {
        brokerConfigurator.Context.Services.TryAddScoped<IOutboxBrokerPublisher, OutboxMassTransitMediatorPublisher>();
    }
}

