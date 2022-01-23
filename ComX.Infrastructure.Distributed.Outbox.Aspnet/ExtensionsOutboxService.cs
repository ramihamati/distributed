using ComX.Infrastructure.Distributed.Outbox;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;
public static class ExtensionsOutboxService
{
    /// <summary>
    /// Creating an isolated IOutboxService by wrapping a boxed <see cref="IServiceCollection"/> inside
    /// an <see cref="IOutboxServiceContainer"/>. This allows us to create multiple instances of the outbox service
    /// which are accesible using the <see cref="IOutboxServiceProvider"/>
    /// 
    /// <para>When using an external dbContext you must tell the container that it must get an external service. To do this 
    /// you must use the method <see cref="ConfiguratorOutboxServiceContainer.UseExternalService{TService}"/>. For more information see the samples section</para>
    /// </summary>
    public static IServiceCollection UseContainerOutboxService(
        this IServiceCollection services,
        string name,
        Action<ConfiguratorOutboxServiceContainer> configurator)
    {
        services.TryAddScoped<IOutboxServiceProvider, OutboxServiceProvider>();

        services.AddScoped<IOutboxServiceContainer>(sp =>
        {
            IServiceCollection boxedServices = new ServiceCollection();
            ConfiguratorOutboxServiceContainer isolatedOutboxConfigurator = new(sp, boxedServices);
            configurator(isolatedOutboxConfigurator);
            return new OutboxServiceContainer(name, boxedServices.BuildServiceProvider());
        });
        return services;
    }

    public static IServiceCollection AddOutboxService(
        this IServiceCollection services,
        Action<ConfiguratorOutboxService> configurator)
    {
        services.TryAddScoped<IOutboxService>(sp =>
        {
            return new OutboxService(
                    sp,
                    sp.GetService<ILoggerFactory>()?.CreateLogger<OutboxService>(),
                    sp.GetRequiredService<IOutboxServiceRegistry>(),
                    sp.GetRequiredService<IEventSerializer>()
                );
        });

        ConfiguratorOutboxService outboxConfiguration = new(services);
        configurator(outboxConfiguration);
        outboxConfiguration.EnsureAllAreRegistered();
        ValidateDeps(services);

        return services;
    }

    private static void ValidateDeps(
        IServiceCollection services)
    {
        Type typeEventSerializer = typeof(IEventSerializer);

        bool hasEventSerializer = services.Any(r => r.ServiceType.Equals(typeEventSerializer));
        if (!hasEventSerializer)
        {
            throw new Exception(@$"You must register an event serializer. Use the method 
{nameof(ConfiguratorOutboxService)}.{nameof(ConfiguratorOutboxService.ConfigureSerializer)}. An event serializer is
present in ComX.Infrastructure.Distributed.Outbox.Masstransit");
        }

//        Type typeStorage = typeof(IOutboxStorage);
//        bool hasStorage = services.Any(r => r.ServiceType.Equals(typeStorage));
//        if (!hasStorage)
//        {
//            throw new Exception(@$"You must register a store service. Use the method 
//{nameof(ConfiguratorOutboxService)}.{nameof(ConfiguratorOutboxService.ConfigureStore)}. A store is
//present in ComX.Infrastructure.Distributed.Outbox.Store.Sql");
//        }
    }
}
