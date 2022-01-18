namespace ComX.Infrastructure.Distributed.Inbox;

/// <summary>
/// THis class is extened in MassTransit implementation.
/// This class is in aspnet because it's used centrally and extened using various frameworks
/// </summary>
public class WorkerConfigurator : IWorkerConfigurator
{
    public ConfiguratorContext Context { get; }

    public WorkerConfigurator(ConfiguratorContext context)
    {
        Context = context;
    }
}
