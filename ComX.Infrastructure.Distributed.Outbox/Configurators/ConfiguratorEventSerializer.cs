namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorEventSerializer : IConfiguratorEventSerializer
{
    public ConfiguratorContext Context { get; }

    public ConfiguratorEventSerializer(ConfiguratorContext context)
    {
        Context = context;
    }
}
