namespace ComX.Infrastructure.Distributed.Outbox;

public class ConfiguratorWorkerStore<TMessageLog> : IConfiguratorWorkerStore<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    public ConfiguratorWorkerContext Context { get; }

    public ConfiguratorWorkerStore(ConfiguratorWorkerContext context)
    {
        Context = context;
    }
}