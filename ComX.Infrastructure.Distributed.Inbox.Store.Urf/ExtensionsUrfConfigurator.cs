using ComX.Infrastructure.Distributed.Inbox;
using ComX.Infrastructure.Distributed.Inbox.Store.Urf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using URF.Core.Abstractions;

namespace ComX.Infrastructure.Distributed.Inbox.Store.Urf;

public static class ExtensionsUrfConfigurator
{
    /// <summary>
    /// Configures the Urf Repository and uses it's internal unit of work
    /// </summary>
    public static void UseUrfRepository(
        this IStoreConfigurator configurator,
        Action<UrfStoreConfigurator> builder)
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        services.TryAddScoped<IStoreProvider, UrfStoreProvider>();
        services.TryAddScoped<IStoreUnitOfWork, StoreUnitOfWork>();

        UrfStoreConfigurator storeConfigurator = new(configurator.Context);
        builder(storeConfigurator);
    }

    /// <summary>
    /// Configures the Urf Repository and uses an external unit of work
    /// </summary>
    public static void UseUrfRepository<TUnitOfWork>(
        this IStoreConfigurator configurator,
        Action<UrfStoreConfigurator> builder) where TUnitOfWork : class, IStoreUnitOfWork
    {
        IServiceCollection services = configurator.Context.Services
            ?? throw new NullReferenceException("The context does not have the services collection");

        services.TryAddScoped<IStoreProvider, UrfStoreProvider>();
        services.TryAddScoped<IStoreUnitOfWork, TUnitOfWork>();

        UrfStoreConfigurator storeConfigurator = new(configurator.Context);
        builder(storeConfigurator);
    }
}
