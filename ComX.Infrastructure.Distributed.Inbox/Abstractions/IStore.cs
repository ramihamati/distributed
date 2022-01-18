namespace ComX.Infrastructure.Distributed.Inbox;

public interface IStore<TModel> where TModel : class
{
    Task SaveAsync(TModel model);
}
