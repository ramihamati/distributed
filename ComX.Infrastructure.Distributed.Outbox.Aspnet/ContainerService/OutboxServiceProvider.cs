using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace ComX.Infrastructure.Distributed.Outbox;

public class OutboxServiceProvider : IOutboxServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxServiceProvider(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IOutboxService GetService(string name)
    {
        return _serviceProvider
           .GetServices<IOutboxServiceContainer>()
           .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.InvariantCultureIgnoreCase))
           ?.ServiceProvider
           .GetService<IOutboxService>();
    }

    public IOutboxServiceContainer GetServiceContainer(string name)
    {
        return _serviceProvider
              .GetServices<IOutboxServiceContainer>()
              .SingleOrDefault(r => string.Equals(r.Name, name, StringComparison.InvariantCultureIgnoreCase));
    }
}
