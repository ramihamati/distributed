using MassTransit;

namespace ComX.Infrastructure.Distributed.Inbox.Masstransit;

public static class ExtensionsWorkerConfigurator
{
    public static void UseMassTransitHostedService(
        this IWorkerConfigurator workerConfigurator)
    {
        workerConfigurator.Context.Services.AddMassTransitHostedService();
    }
}
