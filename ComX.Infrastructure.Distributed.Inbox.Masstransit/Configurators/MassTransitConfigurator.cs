using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public class MassTransitConfigurator : IMassTransitConfigurator
{
    private IEndpointNameFormatter? _endpointNameFormatter;

    
    public IBusRegistrationConfigurator Cfg { get; }

    public MassTransitConfigurator(
        IBusRegistrationConfigurator busRegistrationConfigurator)
    {
        Cfg = busRegistrationConfigurator;
    }
}
