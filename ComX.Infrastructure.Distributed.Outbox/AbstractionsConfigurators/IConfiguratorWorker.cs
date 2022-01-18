namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorWorker
{
    ConfiguratorContext Context { get; }
}
