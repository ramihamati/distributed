namespace ComX.Infrastructure.Distributed.Inbox;

public class TransformerConfigurator : ITransformerConfigurator
{
    public ConfiguratorContext Context { get; }

    public TransformerConfigurator(ConfiguratorContext context)
    {
        Context = context;
    }
}
