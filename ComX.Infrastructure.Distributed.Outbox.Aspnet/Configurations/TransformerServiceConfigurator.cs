using System;
using System.Collections.Generic;

namespace ComX.Infrastructure.Distributed.Outbox;

public class TransformerServiceConfigurator : ITransformerServiceConfigurator
{
    internal Dictionary<Type, Type> Transforms { get; } = new();

    public IConfiguratorTransformer Cfg { get; }

    public TransformerServiceConfigurator(ConfiguratorContext context)
    {
        Cfg = new ConfiguratorTransformer(context);
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
