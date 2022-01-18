using ComX.Infrastructure.Distributed.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsEventSerializerConfigurator
{
    public static void UseMassTransitSerializer(
        this IConfiguratorEventSerializer serializerConfigurator)
    {
        serializerConfigurator.Context.Services.TryAddScoped<IEventSerializer, EventSerializer>();
    }
}

