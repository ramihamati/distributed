using Microsoft.Extensions.DependencyInjection;
using System;

namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorWorkerContext
{
    public IServiceProvider AppServices { get; }
    public IServiceCollection ContainerServices { get; }

    public ConfiguratorWorkerContext(IServiceProvider appServices,IServiceCollection serviceServices)
    {
        AppServices = appServices;
        ContainerServices = serviceServices;
    }
}
