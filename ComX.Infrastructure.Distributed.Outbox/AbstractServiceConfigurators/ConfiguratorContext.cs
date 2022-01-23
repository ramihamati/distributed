using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorContext
{
    public IServiceCollection Services { get; }
    public ConfiguratorContext(IServiceCollection services)
    {
        Services = services;
    }
}
