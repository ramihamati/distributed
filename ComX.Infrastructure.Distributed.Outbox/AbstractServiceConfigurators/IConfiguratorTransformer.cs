namespace ComX.Infrastructure.Distributed.Outbox;

public interface IConfiguratorTransformer
{
    ConfiguratorContext Context { get; }
}