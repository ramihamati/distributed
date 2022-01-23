using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;

public static class ExtensionsTests
{
    public static void UseTestPublisher(
      this IConfiguratorWorkerPublisher brokerConfigurator)
    {
        brokerConfigurator.Context.ContainerServices.TryAddScoped<IOutboxBrokerPublisher, TestPublisher>();
    }
}
