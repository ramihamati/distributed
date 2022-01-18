namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorStore : IConfiguratorStore
{
    public ConfiguratorContext Context { get; }

    public ConfiguratorStore(ConfiguratorContext context)
    {
        Context = context;
    }
}