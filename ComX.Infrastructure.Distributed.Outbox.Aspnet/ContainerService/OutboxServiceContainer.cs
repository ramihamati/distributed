using Microsoft.Extensions.DependencyInjection;
using System;

namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// isolated outbox service
/// </summary>
public class OutboxServiceContainer : IOutboxServiceContainer
{
    public string Name { get; }
    public IServiceProvider ServiceProvider { get; }

    public OutboxServiceContainer(
        string name,
        IServiceProvider serviceProvider)
    {
        Name = name;
        ServiceProvider = serviceProvider;
    }

    public IOutboxService GetService()
    {
        return ServiceProvider.GetService<IOutboxService>();
    }
}
