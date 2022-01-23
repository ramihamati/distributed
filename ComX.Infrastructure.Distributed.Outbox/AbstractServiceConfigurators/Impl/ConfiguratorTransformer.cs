namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorTransformer : IConfiguratorTransformer
{
    public ConfiguratorContext Context { get; }

    public ConfiguratorTransformer(ConfiguratorContext context)
    {
        Context = context;
    }
}
