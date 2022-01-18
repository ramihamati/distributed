namespace ComX.Infrastructure.Distributed.Inbox.Aspnet;

public class TransformerServiceConfigurator : ITransformerServiceConfigurator
{
    internal Dictionary<Type, Type> Transforms { get; } = new();

    public ITransformerConfigurator Cfg { get; }

    public TransformerServiceConfigurator(ConfiguratorContext context)
    {
        Cfg = new TransformerConfigurator(context);
    }

    public void RegisterTransform<TSource, TDestination>()
    {
        if (Transforms.ContainsKey(typeof(TSource)))
        {
            throw new ArgumentException($"A transform for {typeof(TSource) } is already registered");
        }

        Transforms.Add(typeof(TSource), typeof(TDestination));
    }
}
