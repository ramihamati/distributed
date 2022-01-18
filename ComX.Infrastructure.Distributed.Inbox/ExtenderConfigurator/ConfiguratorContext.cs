using Microsoft.Extensions.DependencyInjection;

namespace ComX.Infrastructure.Distributed.Inbox;

public class ConfiguratorContext
{
    public IInboxBusConfigurator? BusConfigurator { get; set; }

    public IEventTypeRegistryBuilder EventTypeRegistryBuilder { get; }

    public IServiceCollection Services { get; }

    public ConfiguratorContext(IServiceCollection services)
    {
        EventTypeRegistryBuilder = new EventTypeRegistryBuilder();
        Services = services;
    }
}
