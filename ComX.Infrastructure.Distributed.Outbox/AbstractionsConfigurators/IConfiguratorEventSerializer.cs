namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorEventSerializer
{
    ConfiguratorContext Context { get; }
}