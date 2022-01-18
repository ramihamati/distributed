using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public class MassTransitBusConfigurator : IInboxBusConfigurator
{
    private readonly IRegistrationConfigurator _configurator;

    public MassTransitBusConfigurator(IRegistrationConfigurator configurator)
    {
        _configurator = configurator;
    }

    public void RegisterConsumer<TEvent>() where TEvent : class
    {
        _configurator.AddConsumer<MassTransitConsumer<TEvent>, DefinitionConsumer<MassTransitConsumer<TEvent>>>();
    }
}
