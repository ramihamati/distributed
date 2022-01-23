namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorWorkerEventSerializer
{
    ConfiguratorWorkerContext Context { get; }
}