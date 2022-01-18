namespace ComX.Infrastructure.Distributed.Inbox;

/// <summary>
/// THis class is extened in MassTransit implementation.
/// This class is in aspnet because it's used centrally and extened using various frameworks
/// </summary>
public class BrokerConfigurator : IBrokerConfigurator
{
    public ConfiguratorContext Context { get; }

    public BrokerConfigurator(ConfiguratorContext context)
    {
        Context = context;
    }
}
