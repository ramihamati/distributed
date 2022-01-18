namespace ComX.Infrastructure.Distributed.Outbox;

/// <summary>
/// THis class is extened in MassTransit implementation.
/// This class is in aspnet because it's used centrally and extened using various frameworks
/// </summary>
public class ConfiguratorWorker : IConfiguratorWorker
{
    public ConfiguratorContext Context { get; }

    public ConfiguratorWorker(ConfiguratorContext context)
    {
        Context = context;
    }
}
