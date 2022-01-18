namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorStore
{
    ConfiguratorContext Context { get; }
}