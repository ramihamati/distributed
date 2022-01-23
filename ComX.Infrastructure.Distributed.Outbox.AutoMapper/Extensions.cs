using ComX.Infrastructure.Distributed.Outbox;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsOutboxAutoMapper
{
    public static void UseAutomapperTransformations(
        this IConfiguratorTransformer configurator)
    {
        configurator
            .Context
            .Services
            .AddScoped<IOutboxTransformer, AutoMapperService>();
    }
}
