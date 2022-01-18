using ComX.Infrastructure.Distributed.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExtensionsConfiguratorStore
{
    /// <summary>
    /// User provides an external repository implementation of <see cref="IOutboxRepository"/> which
    /// is used by the storage internally to persist messages.
    /// </summary>
    public static void UseRepository<TRepository>(this IConfiguratorStore configurator)
      where TRepository : class, IOutboxRepository
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        Type outboxStorageType = typeof(IOutboxStorage);

        if (services.Any(r => r.ServiceType.Equals(outboxStorageType)))
        {
            throw new InvalidOperationException("The store was already added");
        }

        services.AddScoped<IOutboxStorage, OutboxStorageRepository>();
        services.TryAddScoped<IOutboxRepository, TRepository>();
    }
}
