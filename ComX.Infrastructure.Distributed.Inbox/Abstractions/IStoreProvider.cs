namespace ComX.Infrastructure.Distributed.Inbox;

public interface IStoreProvider
{
    IStore<TModel>? GetStore<TModel>() where TModel : class;
}
