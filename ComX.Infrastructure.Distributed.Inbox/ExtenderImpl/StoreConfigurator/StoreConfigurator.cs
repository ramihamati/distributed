namespace ComX.Infrastructure.Distributed.Inbox;

public class StoreConfigurator : IStoreConfigurator
{
    public ConfiguratorContext Context { get; }

    public StoreConfigurator(ConfiguratorContext context)
    {
        Context = context;
    }
}