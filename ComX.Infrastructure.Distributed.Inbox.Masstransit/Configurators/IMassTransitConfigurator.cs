using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public interface IMassTransitConfigurator
{
    IBusRegistrationConfigurator Cfg { get; }
}
