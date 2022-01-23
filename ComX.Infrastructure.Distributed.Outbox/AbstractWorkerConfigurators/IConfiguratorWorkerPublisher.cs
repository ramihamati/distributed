namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorWorkerPublisher
{
    ConfiguratorWorkerContext Context { get; }
}
