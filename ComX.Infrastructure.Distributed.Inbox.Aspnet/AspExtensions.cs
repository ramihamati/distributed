using ComX.Infrastructure.Distributed.Inbox;
using ComX.Infrastructure.Distributed.Inbox.Aspnet;

namespace Microsoft.Extensions.DependencyInjection;

public static class AspExtensions
{
    public static IServiceCollection AddInboxService(
        this IServiceCollection services,
        Action<InboxServiceConfigurator> configurator)
    {
        services.AddScoped<IGenericConsumerAbstractFactory, GenericConsumerAbstractFactory>();

        InboxServiceConfigurator inboxConfigurator = new(services);
        configurator(inboxConfigurator);
        inboxConfigurator.EnsureAllAreRegistered();

        IEventTypeRegistry eventTypeRegistry = inboxConfigurator.Context.EventTypeRegistryBuilder.Build();
        IInboxBusConfigurator inboxBusConfigurator = inboxConfigurator.Context.BusConfigurator ?? throw new Exception($"Bus is not configured. Use the method {nameof(InboxServiceConfigurator.ConfigureBroker)}");
        InboxService inboxService = new(
            eventTypeRegistry,
            inboxBusConfigurator,
            services);

        inboxService.Register();

        return services;
    }
}
