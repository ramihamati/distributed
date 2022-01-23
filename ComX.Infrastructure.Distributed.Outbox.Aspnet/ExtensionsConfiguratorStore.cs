using ComX.Infrastructure.Distributed.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsConfiguratorStore
{
    public static void UseRepository<TRepository>(this IConfiguratorStore configurator)
       where TRepository : class, IOutboxRepository<IntegrationMessageLog>

    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        Type outboxStorageType = typeof(IOutboxStorage<IntegrationMessageLog>);

        if (services.Any(r => r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage<IntegrationMessageLog>, OutboxStorageRepository<IntegrationMessageLog>>();
        services.TryAddScoped<IOutboxRepository<IntegrationMessageLog>, TRepository>();
    }

    /// <summary>
    /// User provides an external repository implementation of <see cref="IOutboxRepository"/> which
    /// is used by the storage internally to persist messages.
    /// </summary>
    public static void UseRepository<TRepository, TMessageLog>(this IConfiguratorStore configurator)
        where TRepository : class, IOutboxRepository<TMessageLog>
        where TMessageLog : class, IIntegrationMessageLog

    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        Type outboxStorageType = typeof(IOutboxStorage<TMessageLog>);

        if (services.Any(r => r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage<TMessageLog>, OutboxStorageRepository<TMessageLog>>();
        services.TryAddScoped<IOutboxRepository<TMessageLog>, TRepository>();
    }
}
