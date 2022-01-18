namespace ComX.Infrastructure.Distributed.Inbox;

public interface IWorkerConfigurator
{
    ConfiguratorContext Context { get; }
}
