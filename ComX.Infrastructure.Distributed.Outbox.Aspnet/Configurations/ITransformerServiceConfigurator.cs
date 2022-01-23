namespace ComX.Infrastructure.Distributed.Outbox;

public interface ITransformerServiceConfigurator
{
    public IConfiguratorTransformer Cfg { get; }
    void RegisterTransform<TSource, TDestination>();
}
