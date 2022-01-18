namespace ComX.Infrastructure.Distributed.Inbox.Aspnet;

public interface ITransformerServiceConfigurator
{
    public ITransformerConfigurator Cfg { get; }
    void RegisterTransform<TSource, TDestination>();
}
