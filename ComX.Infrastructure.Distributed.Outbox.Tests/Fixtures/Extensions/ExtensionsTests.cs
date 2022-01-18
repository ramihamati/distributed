using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComX.Infrastructure.Distributed.Outbox.Tests;

public static class ExtensionsTests
{
    public static void UseTestPublisher(
      this IConfiguratorPublisher brokerConfigurator)
    {
        brokerConfigurator.Context.Services.TryAddScoped<IOutboxBrokerPublisher, TestPublisher>();
    }
}
