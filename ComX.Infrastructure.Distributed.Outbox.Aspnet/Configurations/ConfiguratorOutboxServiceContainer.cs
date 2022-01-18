using Microsoft.Extensions.DependencyInjection;
using System;

namespace ComX.Infrastructure.Distributed.Outbox.Masstransit;

public sealed class ConfiguratorOutboxServiceContainer
{
    private readonly IServiceProvider _mainProvider;
    private readonly IServiceCollection _boxedServices;

    public ConfiguratorOutboxServiceContainer(
        IServiceProvider mainProvider,
        IServiceCollection boxedServices)
    {
        _mainProvider = mainProvider;
        _boxedServices = boxedServices;
    }

    public ConfiguratorOutboxServiceContainer UseExternalService<TService>()
        where TService : class
    {
        _boxedServices.AddScoped<TService>(_ =>
        {
            return _mainProvider.GetRequiredService<TService>();
        });
        return this;
    }

    public ConfiguratorOutboxServiceContainer Configure(Action<ConfiguratorOutboxService> configure)
    {
        _boxedServices.AddOutboxService(configure);
        return this;
    }
}
