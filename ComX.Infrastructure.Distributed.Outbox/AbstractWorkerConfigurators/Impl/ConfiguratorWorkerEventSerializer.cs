namespace ComX.Infrastructure.Distributed.Outbox;


public class ConfiguratorWorkerEventSerializer : IConfiguratorWorkerEventSerializer
{
    public ConfiguratorWorkerContext Context { get; }

    public ConfiguratorWorkerEventSerializer(ConfiguratorWorkerContext context)
    {
        Context = context;
    }
}
