namespace ComX.Infrastructure.Distributed.Inbox.Store.Mongo;

using ComX.Infrastructure.Distributed.Inbox;
using Microsoft.Extensions.DependencyInjection;

public class MongoStoreProvider : IStoreProvider
{

    private readonly IServiceProvider _services;

    public MongoStoreProvider(IServiceProvider services)
    {
        _services = services;
    }

    public IStore<TModel>? GetStore<TModel>() where TModel : class
    {
        return _services.GetRequiredService<IStore<TModel>>();
    }
}
