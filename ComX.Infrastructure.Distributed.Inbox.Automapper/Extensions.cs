using ComX.Infrastructure.Distributed.Inbox;
using ComX.Infrastructure.Distributed.Inbox.Automapper;

namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static void UseAutomapperTransformations(
        this ITransformerConfigurator configurator)
    {
        configurator
            .Context
            .Services
            .AddScoped<ITransformer, AutoMapperService>();
    }
}
