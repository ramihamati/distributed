using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public static class ExtensionsBrokerConfigurator
{
    public static void UseMassTransit(
        this IBrokerConfigurator brokerConfigurator,
        Action<IMassTransitConfigurator> configurator)
    {
        EnsureBrokerIsNull(brokerConfigurator);

        brokerConfigurator.Context.Services.AddMassTransit(cfg =>
        {
            // reference passed to register consumers
            brokerConfigurator.Context.BusConfigurator = new MassTransitBusConfigurator(cfg);
            MassTransitConfigurator consumerConfigurator = new(cfg);
            // external configuration of the broker
            configurator(consumerConfigurator);
        });
    }

    private static void EnsureBrokerIsNull(IBrokerConfigurator brokerConfigurator)
    {
        if (brokerConfigurator.Context.BusConfigurator is not null)
        {
            throw new Exception("Cannot register multiple brokers");
        }
    }
}
