namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorWorkerStore<TMessageLog>
    where TMessageLog : class, IIntegrationMessageLog
{
    ConfiguratorWorkerContext Context { get; }
}